using System;

namespace Radish
{
	public interface IFileViewer
	{
		void InvokeOnMainThread(Action action);
	}
}

