﻿using EWP.SF.KafkaSync.BusinessEntities;
using EWP.SF.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EWP.SF.KafkaSync.BusinessLayer;

public static class DataSyncServiceUtil
{
    public static DateTime ConvertDate(DateTimeFormatType Type, DateTime Date, string TimeZone)
    {
        DateTime dateUtc = Date.ToUniversalTime();
        DateTime response = dateUtc;
        if (Type == DateTimeFormatType.TimeZone && !string.IsNullOrEmpty(TimeZone))
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
            response = TimeZoneInfo.ConvertTimeFromUtc(dateUtc, tz);
        }
        return response;
    }

    public static dynamic MapEntity(string mapSchema, string inputJsonStr, string resultProperty = null, bool ignoreDefaults = false)
    {
        dynamic response = new List<dynamic>();
        List<DataSyncMapSchema> listMapSchema = JsonConvert.DeserializeObject<List<DataSyncMapSchema>>(mapSchema);
        if (inputJsonStr.Trim().StartsWith('['))
        {
            JArray jsonArray = JArray.Parse(inputJsonStr);
            List<JObject> responseArray = [];
            foreach (JObject jObject in jsonArray.Cast<JObject>())
            {
                JToken objSearch = null;
                if (!string.IsNullOrEmpty(resultProperty))
                {
                    _ = jObject.TryGetValue(resultProperty, out objSearch);
                }
                objSearch ??= jObject;
                if (listMapSchema is not null)
                {
                    responseArray.Add(MapProperties(objSearch, listMapSchema, ignoreDefaults, jObject));
                }
            }
            response = responseArray;
        }
        else
        {
            JObject jsResult = JObject.Parse(inputJsonStr);
            JToken objSearch = null;
            if (!string.IsNullOrEmpty(resultProperty))
            {
                _ = jsResult.TryGetValue(resultProperty, out objSearch);
            }
            objSearch ??= jsResult;
            if (listMapSchema is not null)
            {
                response = MapProperties(objSearch, listMapSchema, ignoreDefaults, jsResult);
            }
        }
        return response;
    }

    public static class StaticServiceProvider
    {
        public static IServiceProvider Provider { get; set; }
    }

    public static dynamic MapProperties(JToken objSearch, List<DataSyncMapSchema> listMapSchema, bool ignoreDefault = false, JToken parentToken = null)
    {
        if (objSearch.Type == JTokenType.Array)
        {
            List<dynamic> response = [];
            foreach (JToken e in objSearch.ToArray())
            {
                dynamic elem = MapObject(e, listMapSchema, ignoreDefault, parentToken);
                response.Add(elem);
            }
            return response;
        }
        else if (objSearch.Type == JTokenType.Object)
        {
            return MapObject(objSearch, listMapSchema, ignoreDefault, parentToken);
        }

        return null; // Handle other JToken types if needed
    }
    public static dynamic MapObject(JToken objToken, List<DataSyncMapSchema> listMapSchema, bool ignoreDefault = false, JToken parentToken = null)
{
    dynamic response = new JObject();

    foreach (DataSyncMapSchema f in listMapSchema)
    {
        // Handle parent property references with $parent. notation
        JToken token = null;
        if (string.IsNullOrEmpty(f.OriginProperty))
        {
            // Empty origin property means use only defaultValue
            token = null;
        }
        else if (f.OriginProperty.StartsWith("$parent.") && parentToken != null)
        {
            string parentProperty = f.OriginProperty.Substring(8); // Remove "$parent." prefix
            token = parentToken.SelectToken(parentProperty);
        }
        else
        {
            token = objToken.SelectToken(f.OriginProperty); // ✅ Current object property
        }

        if (f.Type == "string")
        {
            string defaultValue = !string.IsNullOrEmpty(f.DefaultValue) ? f.DefaultValue.Trim() : string.Empty;
            if (ignoreDefault)
                defaultValue = null;

            string value = token?.ToString() ?? defaultValue;

            if (!string.IsNullOrEmpty(defaultValue) && string.IsNullOrEmpty(value))
                value = defaultValue;

            // MappingValues
            if (f.MappingValues != null && f.MappingValues.Any())
            {
                var mapped = f.MappingValues.FirstOrDefault(m => m.ERPValue == value);
                if (mapped != null)
                    value = mapped.SFValue;
            }

            if (DateTime.TryParse(value, out DateTime dateValue) && value.Length == 19)
                value = dateValue.ToString("s");

            response[f.MapProperty] = value?.Trim();
        }

        if (f.Type == "integer")
        {
            int? defaultValue = (f.DefaultValue ?? "0").ToInt32();
            if (ignoreDefault)
                defaultValue = null;

            int value = token?.Value<int>() ?? 0;

            if (Math.Abs(value) < 0.0001 && Math.Abs((double)defaultValue.GetValueOrDefault()) >= 0.0001)
            {
                if (!ignoreDefault)
                    response[f.MapProperty] = defaultValue;
            }
            else
            {
                response[f.MapProperty] = value;
            }
        }

        if (f.Type == "float")
        {
            float.TryParse(f.DefaultValue, out float tempDefault);
            float? defaultValue = ignoreDefault ? null : tempDefault;

            float value = token?.Value<float>() ?? 0;

            if (Math.Abs(value) < 0.0001 && Math.Abs(defaultValue.GetValueOrDefault()) >= 0.0001)
            {
                if (!ignoreDefault)
                    response[f.MapProperty] = defaultValue;
            }
            else
            {
                response[f.MapProperty] = value;
            }
        }

        if (f.Type == "double")
        {
            double? defaultValue = ignoreDefault ? null : f.DefaultValue.ToDouble();
            double? value = token?.Value<double?>();

            if (value is not null)
            {
                if (Math.Abs(value.Value) < 0.0001 && Math.Abs(defaultValue.GetValueOrDefault()) >= 0.0001)
                {
                    if (!ignoreDefault)
                        response[f.MapProperty] = defaultValue;
                }
                else
                {
                    response[f.MapProperty] = value;
                }
            }
            else if (!ignoreDefault)
            {
                response[f.MapProperty] = defaultValue;
            }
        }

        if (f.Type == "decimal")
        {
            decimal? defaultValue = ignoreDefault ? null : f.DefaultValue.ToDecimal();
            decimal value = token?.Value<decimal>() ?? 0;

            if (value == 0 && defaultValue != 0)
            {
                if (!ignoreDefault)
                    response[f.MapProperty] = defaultValue;
            }
            else
            {
                response[f.MapProperty] = value;
            }
        }

        if (f.Type == "bool")
        {
            response[f.MapProperty] = token?.Value<bool>() ?? false;
        }

        if (f.Type == "list")
        {
            JToken value = token ?? new JArray(); // ✅ safe fallback
            // Pass current objToken as parentToken for nested children
            dynamic elemChild = MapProperties(value, f.Children, ignoreDefault, objToken);
            response[f.MapProperty] = JToken.FromObject(elemChild);
        }

        if (f.Type == "datetime")
        {
            if (token != null && DateTime.TryParse(token.ToString(), out DateTime dt))
            {
                response[f.MapProperty] = dt;
            }
        }
    }

    return response;
}

    public static string GetErpErrors(this string input, string name)
    {
        JObject containerToken = JObject.Parse(input);
        List<JToken> matches = [];
        string result;
        try
        {
            FindTokenDetail(containerToken, name, matches);
            if (matches[0].Type.ToString() == "Array")
            {
                List<string> o = matches[0].ToObject<List<string>>();
                result = string.Join('|', o);
            }
            else
            {
                result = matches[0].ToObject<string>();
            }
        }
        catch
        {
            result = string.Empty;
        }
        return result;
    }

    public static List<JToken> FindTokens(this JToken containerToken, string name)
    {
        List<JToken> matches = [];
        FindTokenDetail(containerToken, name, matches);
        return matches;
    }

    private static void FindTokenDetail(JToken containerToken, string name, List<JToken> matches)
    {
        if (containerToken.Type == JTokenType.Object)
        {
            foreach (JProperty child in containerToken.Children<JProperty>())
            {
                if (child.Name == name)
                {
                    matches.Add(child.Value);
                }
                FindTokenDetail(child.Value, name, matches);
            }
        }
        else if (containerToken.Type == JTokenType.Array)
        {
            foreach (JToken child in containerToken.Children())
            {
                FindTokenDetail(child, name, matches);
            }
        }
    }

    public static string FindObjectByPropertyAndValue(DataSyncErpMapping schema, string erpJson, string codeProperty, string codeValue)
    {
        string response = string.Empty;
        JToken findObject = null;

        JObject jsResult = JObject.Parse(erpJson);
        if (!jsResult.TryGetValue(schema.ResultProperty, out JToken objSearch) || objSearch is null)
        {
            objSearch = jsResult;
        }

        if (objSearch.Type == JTokenType.Array)
        {
            foreach (JToken e in objSearch)
            {
                if ((string)e[codeProperty] == codeValue)
                {
                    findObject = e;
                    break;
                }
            }
        }

        if (findObject is not null)
        {
            response = JsonConvert.SerializeObject(findObject);
        }

        return response;
    }
}
