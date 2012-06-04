﻿// Copyright 2012 Alalf <alalf.iQLc_at_gmail.com>
//
// This file is part of SCFF DSF.
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

/// @file scff-app/utilities.cs
/// @brief scff_app用ユーティリティ

namespace scff_app {

using System.Drawing;

// BindingSource用Extensionを使おうと試みたが、無駄なので消した
class Utilities {
  internal static Rectangle GetVirtualDesktopRectangle() {
    int x = ExternalAPI.GetSystemMetrics(ExternalAPI.SM_XVIRTUALSCREEN);
    int y = ExternalAPI.GetSystemMetrics(ExternalAPI.SM_XVIRTUALSCREEN);
    int width = ExternalAPI.GetSystemMetrics(ExternalAPI.SM_CXVIRTUALSCREEN);
    int height = ExternalAPI.GetSystemMetrics(ExternalAPI.SM_CYVIRTUALSCREEN);
    return new Rectangle(x, y, width, height);
  }
}
}