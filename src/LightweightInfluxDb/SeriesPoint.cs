using System.Collections.Generic;

namespace LightweightInfluxDb
{
    /// <summary>
    ///   The obvious implementation of ISeriesPoint.
    ///   You may wish to write a more constrained version to more conveniently match your requirements.
    /// </summary>
    public class SeriesPoint : ISeriesPoint
    {	
	    public string Name { get; set; }
	
    	public Dictionary<string, string> Tags { get; set; }
	
	    public List<string> Fields { get; set; }
	
    	public List<object> Values { get; set; }	
    }
}
