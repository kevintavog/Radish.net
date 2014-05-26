using System;

namespace Radish.Controllers
{
	public interface IFileViewer
	{
		void InvokeOnMainThread(Action action);
	}
}

