## LightweightInfluxDb

This is a .NET library for reading data from and writing data to InfluxDb. As the name suggests, it's very lightweight.

It exists because the alternatives at this point seemed overly complicated for my simple needs.

Available on nuget.

Query Examples:

	var idb = new InfluxDb(url, db, username, password);
	
	var measurements = idb.QuerySingleSeries("show measurements").Select(a => (string)a[0]).ToList();
	
	var q = "select mean(v) from " + seriesName + " where time > "
            	            + (startDate - prePeriod) + "ms and time < " + endDate 
                	        + "ms group by time(" + step + "ms) fill(previous)";
                        
    var values = idb.QuerySingleSeries(q).Select(a => a[1] == null ? null : (double?)Convert.ToDouble(a[1])).ToList();
	
	
Write Example:

	public class MySimpleSeriesPoint : ISeriesPoint
    {
        public MySimpleSeriesPoint(string name, string category, object value)
        {
            Name = name;
            Tags = new Dictionary<string, string> {{"category", category}};
            Fields = new List<string> {"v"};
            Values = new List<object> {value};
        }
        
        public string Name { get; private set; }
        
        public Dictionary<string, string> Tags { get; private set; }
        
        public List<string> Fields { get; private set; }
        
        public List<object> Values { get; private set; }
    }
	
	
	...
	
	
	idb.Write(new MySimpleSeriesPoint("MyMeasurement", "MyCategory", myValue));
	
Note: you could use SeriesPoint rather than a custom implementation of ISeriesPoint, but I think a specialized implementation would often be convenient.

Note: you can also pass a list of ISeriesPoint objects to Write which will generate a multi-line request.
