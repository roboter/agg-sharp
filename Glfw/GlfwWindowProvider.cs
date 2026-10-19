//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
//
// Permission to copy, use, modify, sell and distribute this software
// is granted provided this copyright notice appears in all copies.
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using MatterHackers.Agg.UI;

namespace MatterHackers.GlfwProvider
{
	public class GlfwWindowProvider : ISystemWindowProvider
	{
		private static readonly List<SystemWindow> _openWindows = new List<SystemWindow>();

		public IReadOnlyList<SystemWindow> OpenWindows => _openWindows;

		public SystemWindow TopWindow => _openWindows.LastOrDefault();

		public void ShowSystemWindow(SystemWindow systemWindow)
		{
			if (!_openWindows.Contains(systemWindow))
			{
				_openWindows.Add(systemWindow);
			}

			var platformWindow = new GlfwPlatformWindow
			{
				WindowProvider = this
			};

			systemWindow.PlatformWindow = platformWindow;
			platformWindow.ShowSystemWindow(systemWindow);
		}

		public void CloseSystemWindow(SystemWindow systemWindow)
		{
			_openWindows.Remove(systemWindow);
			if (systemWindow.PlatformWindow is GlfwPlatformWindow glfwPlatformWindow)
			{
				glfwPlatformWindow.CloseWindow();
			}
		}
	}
}