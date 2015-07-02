
namespace IronPythonModule
{
	public static class Extensions
	{
		public static T As<T>(this object self)
		{
			return (T)self;
		}
	}
}

