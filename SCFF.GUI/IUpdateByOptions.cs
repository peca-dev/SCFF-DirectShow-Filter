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

/// @file SCFF.GUI/IUpdateByOptions.cs
/// @copydoc SCFF::GUI::IUpdateByOptions

namespace SCFF.GUI {

/// 自前Options->Controlデータバインディング用インタフェース
///
/// @attention どうしてもINotifyPropertyChangedなどの
///            リフレクションを使いたくなかったためやむなく用意した
interface IUpdateByOptions {
  /// Options->Control書き換え
  void UpdateByOptions();

  /// Options->Controlと同時にControl->Optionsされないように、
  /// Options->Control前にイベントハンドラを取り外す
  void DetachOptionsChangedEventHandlers();

  /// Options->Controlと同時にControl->Optionsされないように、
  /// Options->Control後にイベントハンドラを戻す
  void AttachOptionsChangedEventHandlers();
}
}   // namespace SCFF.GUI
