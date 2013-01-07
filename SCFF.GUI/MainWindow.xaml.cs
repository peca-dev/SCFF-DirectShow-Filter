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

/// @file SCFF.GUI/MainWindow.xaml.cs
/// @copydoc SCFF::GUI::MainWindow

namespace SCFF.GUI {

using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Microsoft.Windows.Shell;
using SCFF.Common;

/// MainWindowのコードビハインド
public partial class MainWindow : Window, IUpdateByProfile, IUpdateByOptions {
  //===================================================================
  // コンストラクタ/Loaded/Closing/ShutdownStartedイベントハンドラ
  //===================================================================

  /// コンストラクタ
  public MainWindow() {    
    this.InitializeComponent();
  }

  /// 全Window表示前に一度だけ起こる
  private void OnLoaded(object sender, RoutedEventArgs e) {
    this.UpdateByOptions();
    this.UpdateByEntireProfile();

    // 必要な機能の実行
    this.SetAero();
    this.SetCompactView();
  }

  /// アプリケーション終了時に発生するClosingイベントハンドラ
  private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
    this.SaveTemporaryOptions();
  }

  //===================================================================
  // イベントハンドラ
  //===================================================================

  /// Deactivated
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnDeactivated(object sender, System.EventArgs e) {
    /// @todo(me) スクリーンキャプチャをの更新頻度を下げる
    ///           App.RuntimeOptionsに該当するデータを保存しておく感じかな？
  }

  /// Activated
  /// @param sender 使用しない
  /// @param e 使用しない
  private void OnActivated(object sender, System.EventArgs e) {
    /// @todo(me) スクリーンキャプチャを更新頻度を元に戻す
    ///           App.RuntimeOptionsに該当するデータを保存しておく感じかな？
  }

  //-------------------------------------------------------------------
  // *Changed/Checked/Unchecked以外
  //-------------------------------------------------------------------

  /// AutoApply: Click
  private void AutoApply_Click(object sender, RoutedEventArgs e) {
    if (!this.AutoApply.IsChecked.HasValue) return;
    App.Options.AutoApply = (bool)this.AutoApply.IsChecked;
  }

  //-------------------------------------------------------------------
  // Checked/Unchecked
  //-------------------------------------------------------------------

  //-------------------------------------------------------------------
  // *Changed/Collapsed/Expanded
  //-------------------------------------------------------------------

  /// AreaExpander: Collapsed
  private void AreaExpander_Collapsed(object sender, RoutedEventArgs e) {
    App.Options.AreaIsExpanded = false;
  }
  /// AreaExpander: Expanded
  private void AreaExpander_Expanded(object sender, RoutedEventArgs e) {
    App.Options.AreaIsExpanded = true;
  }
  /// OptionsExpander: Collapsed
  private void OptionsExpander_Collapsed(object sender, RoutedEventArgs e) {
    App.Options.OptionsIsExpanded = false;
  }
  /// OptionsExpander: Expanded
  private void OptionsExpander_Expanded(object sender, RoutedEventArgs e) {
    App.Options.OptionsIsExpanded = true;
  }
  /// ResizeMethodExpander: Collapsed
  private void ResizeMethodExpander_Collapsed(object sender, RoutedEventArgs e) {
    App.Options.ResizeMethodIsExpanded = false;
  }
  /// ResizeMethodExpander: Expanded
  private void ResizeMethodExpander_Expanded(object sender, RoutedEventArgs e) {
    App.Options.ResizeMethodIsExpanded = true;
  }
  /// LayoutExpander: Collapsed
  private void LayoutExpander_Collapsed(object sender, RoutedEventArgs e) {
    App.Options.LayoutIsExpanded = false;

    UpdateCommands.UpdateLayoutEditByOptions.Execute(null, null);
  }
  /// LayoutExpander: Expanded
  private void LayoutExpander_Expanded(object sender, RoutedEventArgs e) {
    App.Options.LayoutIsExpanded = true;

    UpdateCommands.UpdateLayoutEditByOptions.Execute(null, null);
  }

  //===================================================================
  // IUpdateByOptionsの実装
  //===================================================================

  /// @copydoc IUpdateByOptions::UpdateByOptions
  public void UpdateByOptions() {
    // Temporary
    this.Left         = App.Options.TmpMainWindowLeft;
    this.Top          = App.Options.TmpMainWindowTop;
    this.Width        = App.Options.TmpMainWindowWidth;
    this.Height       = App.Options.TmpMainWindowHeight;
    this.WindowState  = (System.Windows.WindowState)App.Options.TmpMainWindowState;
    
    // MainWindow.Controls
    this.AutoApply.IsChecked = App.Options.AutoApply;
    this.DetachOptionsChangedEventHandlers();
    this.AreaExpander.IsExpanded          = App.Options.AreaIsExpanded;
    this.OptionsExpander.IsExpanded       = App.Options.OptionsIsExpanded;
    this.ResizeMethodExpander.IsExpanded  = App.Options.ResizeMethodIsExpanded;
    this.LayoutExpander.IsExpanded        = App.Options.LayoutIsExpanded;
    this.AttachOptionsChangedEventHandlers();

    // UserControls
    this.LayoutToolbar.UpdateByOptions();
    this.LayoutEdit.UpdateByOptions();
    this.MainMenu.UpdateByOptions();
  }

  /// @copydoc IUpdateByOptions::DetachOptionsChangedEventHandlers
  public void DetachOptionsChangedEventHandlers() {
    this.AreaExpander.Collapsed -= this.AreaExpander_Collapsed;
    this.AreaExpander.Expanded -= this.AreaExpander_Expanded;
    this.OptionsExpander.Collapsed -= this.OptionsExpander_Collapsed;
    this.OptionsExpander.Expanded -= this.OptionsExpander_Expanded;
    this.ResizeMethodExpander.Collapsed -= this.ResizeMethodExpander_Collapsed;
    this.ResizeMethodExpander.Expanded -= this.ResizeMethodExpander_Expanded;
    this.LayoutExpander.Collapsed -= this.LayoutExpander_Collapsed;
    this.LayoutExpander.Expanded -= this.LayoutExpander_Expanded;
  }

  /// @copydoc IUpdateByOptions::AttachOptionsChangedEventHandlers
  public void AttachOptionsChangedEventHandlers() {
    this.AreaExpander.Collapsed += this.AreaExpander_Collapsed;
    this.AreaExpander.Expanded += this.AreaExpander_Expanded;
    this.OptionsExpander.Collapsed += this.OptionsExpander_Collapsed;
    this.OptionsExpander.Expanded += this.OptionsExpander_Expanded;
    this.ResizeMethodExpander.Collapsed += this.ResizeMethodExpander_Collapsed;
    this.ResizeMethodExpander.Expanded += this.ResizeMethodExpander_Expanded;
    this.LayoutExpander.Collapsed += this.LayoutExpander_Collapsed;
    this.LayoutExpander.Expanded += this.LayoutExpander_Expanded;
  }

  /// UIから設定にデータを保存
  private void SaveTemporaryOptions() {
    // Tmp接頭辞のプロパティだけはここで更新する必要がある
    var isNormal = this.WindowState == System.Windows.WindowState.Normal;
    App.Options.TmpMainWindowLeft = isNormal ? this.Left : this.RestoreBounds.Left;
    App.Options.TmpMainWindowTop = isNormal ? this.Top : this.RestoreBounds.Top;
    App.Options.TmpMainWindowWidth = isNormal ? this.Width : this.RestoreBounds.Width;
    App.Options.TmpMainWindowHeight = isNormal ? this.Height : this.RestoreBounds.Height;
    App.Options.TmpMainWindowState = (SCFF.Common.WindowState)this.WindowState;
  }

  //===================================================================
  // IUpdateByProfileの実装
  //===================================================================

  /// @copydoc IUpdateByProfile::UpdateByCurrentProfile
  public void UpdateByCurrentProfile() {
    this.TargetWindow.UpdateByCurrentProfile();
    this.Area.UpdateByCurrentProfile();
    this.Options.UpdateByCurrentProfile();
    this.ResizeMethod.UpdateByCurrentProfile();
    this.LayoutParameter.UpdateByCurrentProfile();
    this.LayoutTab.UpdateByCurrentProfile();
    this.LayoutEdit.UpdateByCurrentProfile();
  }

  /// @copydoc IUpdateByProfile::UpdateByEntireProfile
  public void UpdateByEntireProfile() {
    this.TargetWindow.UpdateByEntireProfile();
    this.Area.UpdateByEntireProfile();
    this.Options.UpdateByEntireProfile();
    this.ResizeMethod.UpdateByEntireProfile();
    this.LayoutParameter.UpdateByEntireProfile();
    this.LayoutTab.UpdateByEntireProfile();
    this.LayoutEdit.UpdateByEntireProfile();
  }

  /// @copydoc IUpdateByProfile::UpdateByEntireProfile
  public void AttachProfileChangedEventHandlers() {
    // nop
  }

  /// @copydoc IUpdateByProfile::UpdateByEntireProfile
  public void DetachProfileChangedEventHandlers() {
    // nop
  }

  //===================================================================
  // コマンドイベントハンドラ
  //===================================================================

  //-------------------------------------------------------------------
  // ApplicationCommands
  //-------------------------------------------------------------------

  /// New
  /// @param sender 使用しない
  /// @param e 使用しない
  private void New_Executed(object sender, ExecutedRoutedEventArgs e) {
    var result = MessageBox.Show("Do you want to save changes?",
                                 "SCFF.GUI",
                                 MessageBoxButton.YesNoCancel,
                                 MessageBoxImage.Warning,
                                 MessageBoxResult.Yes);
    switch (result) {
      case MessageBoxResult.No: {
        App.Profile.ResetProfile();
        this.UpdateByEntireProfile();
        break;
      }
      case MessageBoxResult.Yes: {
        var save = new SaveFileDialog();
        save.Title = "SCFF.GUI";
        save.Filter = "SCFF.GUI Profile|*.SCFF.GUI.profile";
        var saveResult = save.ShowDialog();
        if (saveResult.HasValue && (bool)saveResult) {
          /// @todo(me) 実装
          MessageBox.Show(save.FileName);
          App.Options.AddRecentProfile(save.FileName);
          this.MainMenu.UpdateByOptions();

          App.Profile.ResetProfile();
          this.UpdateByEntireProfile();
        }
        break;
      }
    }
  }

  /// Open
  /// @param sender 使用しない
  /// @param e 使用しない
  private void Open_Executed(object sender, ExecutedRoutedEventArgs e) {
    /// @todo(me) Newと似たコードが必要だがかなりめんどくさい。あとでかく
  }

  /// Save
  /// @param sender 使用しない
  /// @param e 使用しない
  private void Save_Executed(object sender, ExecutedRoutedEventArgs e) {
    /// @todo(me) すでに保存されていない場合はダイアログをだす
    var save = new SaveFileDialog();
    save.Title = "SCFF.GUI";
    save.Filter = "SCFF.GUI Profile|*.SCFF.GUI.profile";
    var saveResult = save.ShowDialog();
    if (saveResult.HasValue && (bool)saveResult) {
      /// @todo(me) 実装
      MessageBox.Show(save.FileName);
      App.Options.AddRecentProfile(save.FileName);
      this.MainMenu.UpdateByOptions();
    }
  }

  /// SaveAs
  /// @param sender 使用しない
  /// @param e 使用しない
  private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e) {
    var save = new SaveFileDialog();
    save.Title = "SCFF.GUI";
    save.Filter = "SCFF.GUI Profile|*.SCFF.GUI.profile";
    var saveResult = save.ShowDialog();
    if (saveResult.HasValue && (bool)saveResult) {
      /// @todo(me) 実装
      MessageBox.Show(save.FileName);
      App.Options.AddRecentProfile(save.FileName);
      this.MainMenu.UpdateByOptions();
    }
  }

  //-------------------------------------------------------------------
  // Windows.Shell.SystemCommands
  //-------------------------------------------------------------------
  
  /// CloseWindow
	private void CloseWindow_Executed(object sender, ExecutedRoutedEventArgs e) {
		SystemCommands.CloseWindow(this);
	}
  /// MaximizeWindow
	private void MaximizeWindow_Executed(object sender, ExecutedRoutedEventArgs e) {
		SystemCommands.MaximizeWindow(this);
	}
  /// MinimizeWindow
	private void MinimizeWindow_Executed(object sender, ExecutedRoutedEventArgs e) {
		SystemCommands.MinimizeWindow(this);
	}
  /// RestoreWindow
	private void RestoreWindow_Executed(object sender, ExecutedRoutedEventArgs e) {
		SystemCommands.RestoreWindow(this);
	}

  //-------------------------------------------------------------------
  // SCFF.GUI.UpdateCommands
  //-------------------------------------------------------------------

  /// @copydoc SCFF::GUI::UpdateCommands::UpdateMainWindowByEntireProfile
  /// @param sender 使用しない
  /// @param e 使用しない
  private void UpdateMainWindowByEntireProfile_Executed(object sender, ExecutedRoutedEventArgs e) {
    this.UpdateByEntireProfile();
  }
  /// @copydoc SCFF::GUI::UpdateCommands::UpdateLayoutEditByEntireProfile
  /// @param sender 使用しない
  /// @param e 使用しない
  private void UpdateLayoutEditByEntireProfile_Executed(object sender, ExecutedRoutedEventArgs e) {
    this.LayoutEdit.UpdateByEntireProfile();
  }
  /// @copydoc SCFF::GUI::UpdateCommands::UpdateLayoutEditByCurrentProfile
  /// @param sender 使用しない
  /// @param e 使用しない
  private void UpdateLayoutEditByCurrentProfile_Executed(object sender, ExecutedRoutedEventArgs e) {
    this.LayoutEdit.UpdateByCurrentProfile();
  }
  /// @copydoc SCFF::GUI::UpdateCommands::UpdateTargetWindowByCurrentProfile
  /// @param sender 使用しない
  /// @param e 使用しない
  private void UpdateTargetWindowByCurrentProfile_Executed(object sender, ExecutedRoutedEventArgs e) {
    this.TargetWindow.UpdateByCurrentProfile();
    this.Area.UpdateByCurrentProfile();
    this.LayoutEdit.UpdateByCurrentProfile();
    this.LayoutParameter.UpdateByCurrentProfile();
  }
  /// @copydoc SCFF::GUI::UpdateCommands::UpdateLayoutParameterByCurrentProfile
  /// @param sender 使用しない
  /// @param e 使用しない
  private void UpdateLayoutParameterByCurrentProfile_Executed(object sender, ExecutedRoutedEventArgs e) {
    this.LayoutParameter.UpdateByCurrentProfile();
  }
  /// @copydoc SCFF::GUI::UpdateCommands::UpdateLayoutEditByOptions
  /// @param sender 使用しない
  /// @param e 使用しない
  private void UpdateLayoutEditByOptions_Executed(object sender, ExecutedRoutedEventArgs e) {
    this.LayoutEdit.UpdateByOptions();
  }

  //-------------------------------------------------------------------
  // SCFF.GUI.Commands
  //-------------------------------------------------------------------

  /// AeroをON/OFF
  private void SetAero() {
    // @todo(me) 実装
  }

  /// AeroのON/OFFが可能か
  private bool CanUseAero() {
    // @todo(me) 実装
    return true;
  }

  /// @copydoc SetAero
  /// @param sender 使用しない
  /// @param e 使用しない
  private void SetAero_Executed(object sender, ExecutedRoutedEventArgs e) {
    Debug.WriteLine("Command [SetAero]:");
    this.SetAero();
  }

  /// @copydoc CanUseAero
  /// @param sender 使用しない
  /// @param[out] e 実行可能か(CanExecute)を設定可能
  private void SetAero_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
    e.CanExecute = this.CanUseAero();
  }

  //-------------------------------------------------------------------

  /// コンパクト表示切替
  private void SetCompactView() {
    if (App.Options.CompactView) {
      this.OptionsExpander.Visibility = Visibility.Collapsed;
      this.ResizeMethodExpander.Visibility = Visibility.Collapsed;
      this.LayoutExpander.IsExpanded = false;
      this.Width = Constants.CompactMainWindowWidth;
      this.Height = Constants.CompactMainWindowHeight;
    } else {
      this.OptionsExpander.Visibility = Visibility.Visible;
      this.ResizeMethodExpander.Visibility = Visibility.Visible;
    }
  }

  /// @copydoc SetCompactView
  /// @param sender 使用しない
  /// @param e 使用しない
  private void SetCompactView_Executed(object sender, ExecutedRoutedEventArgs e) {
    Debug.WriteLine("Command [SetCompactView]:");
    this.SetCompactView();
  }
}
}   // namespace SCFF.GUI
