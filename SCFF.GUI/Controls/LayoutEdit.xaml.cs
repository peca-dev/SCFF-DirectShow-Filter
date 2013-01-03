﻿// Copyright 2012-2013 Alalf <alalf.iQLc_at_gmail.com>
//
// This file is part of SCFF-DirectShow-Filter(SCFF DSF).
//
// SCFF DSF is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SCFF DSF is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with SCFF DSF.  If not, see <http://www.gnu.org/licenses/>.

/// @file SCFF.GUI/Controls/LayoutEdit.xaml.cs
/// レイアウトエディタコントロール

/// SCFF.GUIのユーザコントロールをまとめた名前空間
namespace SCFF.GUI.Controls {

using SCFF.Common;
using SCFF.Common.GUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

/// レイアウトエディタコントロール
///
/// LayoutEditImage内の座標系は([0-100],[0-100])で固定（プレビューのサイズに依存しない）
/// 逆に言うと依存させてはいけない
public partial class LayoutEdit : UserControl, IUpdateByProfile, IUpdateByOptions {

  //===================================================================
  // 定数
  //===================================================================  

  // 1.0のままやると値が小さすぎてフォントがバグるので100倍
  private const double Scale = 100.0;

  private const double MaxImageWidth = 1.0 * Scale;
  private const double MaxImageHeight = 1.0 * Scale;
  private const double PenThickness = 0.005 * Scale;
  private const double CaptionSize = 0.03 * Scale;
  private const double CaptionMargin = PenThickness;

  //===================================================================
  // privateメンバ
  //===================================================================

  /// プレビューサイズを決めるRect
  private Rect previewRect = new Rect(0.0, 0.0, MaxImageWidth, MaxImageHeight);

  /// スクリーンキャプチャ用スレッド管理クラスのインスタンス
  private ScreenCapturer screenCapturer = null;

  //===================================================================
  // コンストラクタ/Loaded/ShutdownStartedイベントハンドラ
  //===================================================================

  /// コンストラクタ
  public LayoutEdit() {
    InitializeComponent();
    this.Dispatcher.ShutdownStarted += OnShutdownStarted;

    /// @todo(me) App.RuntimeOptionsからの値の取得
    this.LayoutEditViewBox.Width = Constants.DummyPreviewWidth;
    this.LayoutEditViewBox.Height = Constants.DummyPreviewHeight;
    RenderOptions.SetBitmapScalingMode(this.DrawingGroup, BitmapScalingMode.LowQuality);

    // Clipping
    // 重くないか？
    this.DrawingGroup.ClipGeometry = new RectangleGeometry(this.previewRect);
    this.DrawingGroup.ClipGeometry.Freeze();

    // スクリーンキャプチャマネージャの準備
    this.screenCapturer = new ScreenCapturer(bitmapsUpdateTimerPeriod);
    this.screenCapturer.Start();

    // BitmapSource更新用タイマーの準備
    this.StartBitmapsUpdateTimer();
  }

  /// Loadedイベントハンドラ
  void OnLoaded(object sender, RoutedEventArgs e) {
    Debug.WriteLine("LayoutEdit: OnLoaded");
  }

  /// Dispatcher.ShutdownStartedイベントハンドラ
  private void OnShutdownStarted(object sender, EventArgs e) {
    Debug.WriteLine("LayoutEdit: ShutdownStarted");
    this.screenCapturer.End();
  }

  //===================================================================
  // this.DrawingGroupへの描画
  //===================================================================

  /// レイアウト要素の描画範囲を求める
  private Rect CreateLayoutElementRect(Profile.InputLayoutElement layoutElement) {
    return new Rect() {
      X = layoutElement.BoundRelativeLeft * MaxImageWidth,
      Y = layoutElement.BoundRelativeTop * MaxImageHeight,
      Width = (layoutElement.BoundRelativeRight - layoutElement.BoundRelativeLeft) * MaxImageWidth,
      Height = (layoutElement.BoundRelativeBottom - layoutElement.BoundRelativeTop) * MaxImageHeight
    };
  }

  private FormattedText CreateLayoutElementCaption(Profile.InputLayoutElement layoutElement) {
    var isCurrent = layoutElement.Index == App.Profile.CurrentInputLayoutElement.Index;

    // Caption
    // サンプル: [1] (640x480) WindowCaption
    var layoutElementCaption = "[" + (layoutElement.Index+1) + "] "; 
    if (isCurrent) {
      /// @todo(me) ピクセル単位の幅と高さの出力
      layoutElementCaption += layoutElement.WindowCaption;
    } else {
      // Currentでなければ[1]以外は表示する必要はない
    }
    
    // Brush
    Brush textBrush = null;
    switch (layoutElement.WindowType) {
      case WindowTypes.Normal: {
        textBrush = isCurrent ? BrushesAndPens.CurrentNormalBrush
                              : BrushesAndPens.NormalBrush;
        break;
      }
      case WindowTypes.DesktopListView: {
        textBrush = isCurrent ? BrushesAndPens.CurrentDesktopListViewBrush
                              : BrushesAndPens.DesktopListViewBrush;
        break;
      }
      case WindowTypes.Desktop: {
        textBrush = isCurrent ? BrushesAndPens.CurrentDesktopBrush
                              : BrushesAndPens.DesktopBrush;
        break;
      }
    }

    // FormattedText
    var formattedText = new FormattedText(layoutElementCaption,
        System.Globalization.CultureInfo.CurrentUICulture,
        FlowDirection.LeftToRight,
        new Typeface("Meiryo"),
        CaptionSize,
        textBrush);
    formattedText.MaxTextWidth = layoutElement.BoundRelativeWidth * Scale;
    formattedText.MaxLineCount = 1;
    return formattedText;
  }

  private void DrawPreview(DrawingContext dc, Profile.InputLayoutElement layoutElement) {
    if (this.capturedBitmaps[layoutElement.Index] == null) return;

    // プレビューの描画
    var layoutElementRect = this.CreateLayoutElementRect(layoutElement);
    dc.DrawImage(this.capturedBitmaps[layoutElement.Index], layoutElementRect);
  }

  private void DrawBorder(DrawingContext dc, Profile.InputLayoutElement layoutElement) {
    var isCurrent = layoutElement.Index == App.Profile.CurrentInputLayoutElement.Index;

    // Pen
    Pen framePen = null;
    switch (layoutElement.WindowType) {
      case WindowTypes.Normal: {
        framePen = isCurrent ? BrushesAndPens.CurrentNormalPen
                             : BrushesAndPens.NormalPen;
        break;
      }
      case WindowTypes.DesktopListView: {
        framePen = isCurrent ? BrushesAndPens.CurrentDesktopListViewPen
                             : BrushesAndPens.DesktopListViewPen;
        break;
      }
      case WindowTypes.Desktop: {
        framePen = isCurrent ? BrushesAndPens.CurrentDesktopPen
                             : BrushesAndPens.DesktopPen;
        break;
      }
    }

    // フレームの描画
    var layoutElementRect = this.CreateLayoutElementRect(layoutElement);
    dc.DrawRectangle(Brushes.Transparent, framePen, layoutElementRect);

    // キャプションの描画
    var layoutElementCaption = this.CreateLayoutElementCaption(layoutElement);
    var captionPoint = new Point(layoutElementRect.X + CaptionMargin, layoutElementRect.Y + CaptionMargin);

    // キャプションから縁取りを取得
    /// @todo(me) 若干重い？
    var textGeometry = layoutElementCaption.BuildGeometry(captionPoint);
    dc.DrawGeometry(null, BrushesAndPens.DropShadowPen, textGeometry);

    dc.DrawText(layoutElementCaption, captionPoint);
  }

  /// 描画テスト用
  /// @todo(me) FPS制限が必要かも？でもあんまりかわらないかも
  private void DrawProfile() {
    using (var dc = this.DrawingGroup.Open()) {
      // 背景描画でサイズを決める
      dc.DrawRectangle(Brushes.Black, null, this.previewRect);

      // プレビューを下に描画
      if (App.Options.LayoutPreview) {
        foreach (var layoutElement in App.Profile) {
          this.DrawPreview(dc, layoutElement);
        }
      }

      // 枠線とキャプションを描画
      if (App.Options.LayoutBorder) {
        foreach (var layoutElement in App.Profile) {
          this.DrawBorder(dc, layoutElement);
        }
      }
    }
  }

  //===================================================================
  // DispatcherTimerによるBitmapSourceの更新
  //===================================================================

  /// BitmapSource更新間隔: 3000ミリ秒
  private const double bitmapsUpdateTimerPeriod = 3000;
  /// BitmapSourceを更新するためのDispatcherTimer
  private DispatcherTimer bitmapsUpdateTimer = new DispatcherTimer();
  /// ScreenCapturer から受け取ったデータを格納しておく
  private BitmapSource[] capturedBitmaps = new BitmapSource[Constants.MaxLayoutElementCount];

  private void StartBitmapsUpdateTimer() {
    bitmapsUpdateTimer.Interval = TimeSpan.FromMilliseconds(bitmapsUpdateTimerPeriod);
    bitmapsUpdateTimer.Tick += bitmapsUpdateTimer_Tick;
    bitmapsUpdateTimer.Start();
  }

  private BitmapSource CreateBitmapSource(int index) {
    var result = this.screenCapturer.GetResult(index);
    if (result == null) return null;
    var bitmapSource = BitmapSource.Create(
        result.PixelWidth, result.PixelHeight,
        result.DpiX, result.DpiY,
        PixelFormats.Bgr32, null,
        result.Pixels, result.Stride);
    /// @todo(me) result.Pixelsを開放する方法はないだろうか？
    return bitmapSource;
  }

  void bitmapsUpdateTimer_Tick(object sender, EventArgs e) {
    this.capturedBitmaps = new BitmapSource[Constants.MaxLayoutElementCount];
    foreach (var layoutElement in App.Profile) {
      this.capturedBitmaps[layoutElement.Index] =
          this.CreateBitmapSource(layoutElement.Index);
    }
    GC.Collect();
    // プレビュー更新
    this.DrawProfile();
  }

  //===================================================================
  // IUpdateByProfileの実装
  //===================================================================

  /// @copydoc IUpdateByProfile.UpdateByCurrentProfile
  public void UpdateByCurrentProfile() {
    this.SendRequestToScreenCapturer(App.Profile.CurrentInputLayoutElement);
    this.DrawProfile();
  }

  /// @copydoc IUpdateByProfile.UpdateByEntireProfile
  public void UpdateByEntireProfile() {
    this.screenCapturer.ClearRequests();
    foreach (var layoutElement in App.Profile) {
      this.SendRequestToScreenCapturer(layoutElement);
    }
    this.DrawProfile();
  }

  /// @copydoc IUpdateByProfile.UpdateByEntireProfile
  public void AttachProfileChangedEventHandlers() {
    // nop
  }

  /// @copydoc IUpdateByProfile.UpdateByEntireProfile
  public void DetachProfileChangedEventHandlers() {
    // nop
  }

  /// LayoutElementの内容からRequestを生成してScreenCapturerに画像生成を依頼
  private void SendRequestToScreenCapturer(Profile.InputLayoutElement layoutElement) {
    var request = new ScreenCaptureRequest {
      Index = layoutElement.Index,
      Window = layoutElement.Window,
      ClippingX = layoutElement.WindowType == WindowTypes.Desktop ?
                      layoutElement.ScreenClippingXWithFit :
                      layoutElement.ClippingXWithFit,
      ClippingY = layoutElement.WindowType == WindowTypes.Desktop ?
                      layoutElement.ScreenClippingYWithFit :
                      layoutElement.ClippingYWithFit,
      ClippingWidth = layoutElement.ClippingWidthWithFit,
      ClippingHeight = layoutElement.ClippingHeightWithFit,
      ShowCursor = layoutElement.ShowCursor,
      ShowLayeredWindow = layoutElement.ShowLayeredWindow
    };
    this.screenCapturer.SendRequest(request);
  }

  //===================================================================
  // IUpdateByOptionsの実装
  //===================================================================

  /// @copydoc IUpdateByOptions.UpdateByOptions
  public void UpdateByOptions() {
    /// @todo(me) LayoutPreview変更時にこのUpdateByOptionsが呼ばれるように変更する
    ///           具体的には新しいUpdateCommandsを作成する
    if (App.Options.LayoutIsExpanded && App.Options.LayoutPreview) {
      this.screenCapturer.Resume();
    } else {
      this.screenCapturer.Suspend();
    }
  }
  
  /// @copydoc IUpdateByOptions.DetachOptionsChangedEventHandlers
  public void DetachOptionsChangedEventHandlers() {
    // nop
  }

  /// @copydoc IUpdateByOptions.AttachOptionsChangedEventHandlers
  public void AttachOptionsChangedEventHandlers() {
    // nop
  }


  //===================================================================
  // イベントハンドラ
  //===================================================================

  /// カーソルをまとめたディクショナリ
  public readonly Dictionary<HitModes, Cursor> hitModesToCursors =
      new Dictionary<HitModes,Cursor> {
    {HitModes.Neutral, null},
    {HitModes.Move, Cursors.SizeAll},
    {HitModes.SizeNW, Cursors.SizeNWSE},
    {HitModes.SizeNE, Cursors.SizeNESW},
    {HitModes.SizeSW, Cursors.SizeNESW},
    {HitModes.SizeSE, Cursors.SizeNWSE},
    {HitModes.SizeN, Cursors.SizeNS},
    {HitModes.SizeW, Cursors.SizeWE},
    {HitModes.SizeS, Cursors.SizeNS},
    {HitModes.SizeE, Cursors.SizeWE}
  };

  /// マウスポインタとLeft/Right/Top/BottomのOffset
  /// @todo(me) MoveAndSizeStateとしてまとめられないだろうか？
  private RelativeMouseOffset relativeMouseOffset = null;
  /// スナップガイド
  /// @todo(me) MoveAndSizeStateとしてまとめられないだろうか？
  private SnapGuide snapGuide = null;
  /// ヒットテストの結果
  /// @todo(me) MoveAndSizeStateとしてまとめられないだろうか？
  private HitModes hitMode = HitModes.Neutral;

  /// マウスポインタを(0.0-1.0, 0.0-1.0)のRelativePointに変換
  private RelativePoint GetRelativeMousePoint(IInputElement image, MouseEventArgs e) {
    var mousePoint = e.GetPosition(image);
    return new RelativePoint(mousePoint.X / MaxImageWidth, mousePoint.Y / MaxImageHeight);
  }

  /// MouseDownイベントハンドラ 
  private void LayoutEditImage_MouseDown(object sender, MouseButtonEventArgs e) {
    // 前処理
    e.Handled = true;
    var image = (IInputElement)sender;
    var relativeMousePoint = this.GetRelativeMousePoint(image, e);

    // HitTest
    int hitIndex;
    Common.GUI.HitModes hitMode;
    if (!Common.GUI.HitTest.TryHitTest(App.Profile, relativeMousePoint, out hitIndex, out hitMode)) return;

    // 現在選択中のIndexではない場合はそれに変更する
    if (hitIndex != App.Profile.CurrentInputLayoutElement.Index) {
      Debug.WriteLine("*****LayoutEdit: Change Current*****");
      Debug.WriteLine("{0:D}->{1:D} ({2:F2}, {3:F2})",
                      App.Profile.CurrentInputLayoutElement.Index,
                      hitIndex,
                      relativeMousePoint.X, relativeMousePoint.Y);

      App.Profile.ChangeCurrentIndex(hitIndex);
      UpdateCommands.UpdateMainWindowByEntireProfile.Execute(null, null);
    }

    // マウスを押した場所を記録してマウスキャプチャー開始
    this.hitMode = hitMode;
    this.relativeMouseOffset = new Common.GUI.RelativeMouseOffset(App.Profile.CurrentInputLayoutElement, relativeMousePoint);
    this.snapGuide = new Common.GUI.SnapGuide(App.Profile, App.Options.LayoutSnap);
    image.CaptureMouse();

    this.DrawProfile();
  }

  /// MouseMoveイベントハンドラ
  private void LayoutEditImage_MouseMove(object sender, MouseEventArgs e) {
    // 前処理
    e.Handled = true;
    var image = (IInputElement)sender;
    var relativeMousePoint = this.GetRelativeMousePoint(image, e);

    // Neutralのときだけはカーソルを帰るだけ
    if (this.hitMode == Common.GUI.HitModes.Neutral) {
      // カーソルかえるだけ
      int hitIndex;
      Common.GUI.HitModes hitMode;
      Common.GUI.HitTest.TryHitTest(App.Profile, relativeMousePoint, out hitIndex, out hitMode);
      this.Cursor = this.hitModesToCursors[hitMode];
      return;
    }

    // Move/Size*
    double nextLeft = -1.0;
    double nextTop = -1.0;
    double nextRight = -1.0;
    double nextBottom = -1.0;

    if (this.hitMode == HitModes.Move) {
      // Move
      MoveAndSize.Move(App.Profile.CurrentInputLayoutElement,
          relativeMousePoint, this.relativeMouseOffset, this.snapGuide,
          out nextLeft, out nextTop, out nextRight, out nextBottom);
    } else {
      // Size*
      MoveAndSize.Size(App.Profile.CurrentInputLayoutElement,
          relativeMousePoint, this.relativeMouseOffset, this.snapGuide, this.hitMode,
          out nextLeft, out nextTop, out nextRight, out nextBottom);
    }

    // Profileを更新
    App.Profile.CurrentOutputLayoutElement.BoundRelativeLeft = nextLeft;
    App.Profile.CurrentOutputLayoutElement.BoundRelativeTop = nextTop;
    App.Profile.CurrentOutputLayoutElement.BoundRelativeRight = nextRight;
    App.Profile.CurrentOutputLayoutElement.BoundRelativeBottom = nextBottom;
      
    /// @todo(me) 変更をMainWindowに通知
    UpdateCommands.UpdateLayoutParameterByCurrentProfile.Execute(null, null);

    this.DrawProfile();
  }

  /// MouseUpイベントハンドラ
  private void LayoutEditImage_MouseUp(object sender, MouseButtonEventArgs e) {
    e.Handled = true;
    if (this.hitMode != HitModes.Neutral) {
      this.LayoutEditImage.ReleaseMouseCapture();
      this.hitMode = HitModes.Neutral;
      this.DrawProfile();
    }
  }
}
}
