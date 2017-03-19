using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Hero
{
    [JsonProperty("name")]
    public string name { get; set; }
    [JsonProperty("id")]
    public int id { get; set; }
    [JsonProperty("localized_name")]
    public string localized_name { get; set; }
}

public class Result
{

    public List<Hero> heroes { get; set; }
    public int status { get; set; }
    public int count { get; set; }
}

/*
public class RootObject
{


    //[JsonProperty("username")]
    public Result result { get; set; }
}
*/
