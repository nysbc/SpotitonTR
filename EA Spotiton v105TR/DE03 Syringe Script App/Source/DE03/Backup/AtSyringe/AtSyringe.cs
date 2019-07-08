using System;

namespace Aurigin
{
	/// <summary>
	/// Summary description for AtSyringe.
	/// </summary>
	public class AtSyringe
	{
		static string configFile = @"..\data\syringe.xml";
		public static UserSyringeControl Control = null;

		static AtSyringe()
		{
			try
			{
				Control = new UserSyringeControl(configFile);
			} 
			catch (Exception e)
			{
				Console.WriteLine(configFile + "configuration file not found :\n" + e.Message);
			}
		}
	}
}
