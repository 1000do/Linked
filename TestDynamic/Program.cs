using System;
using System.Dynamic;

public class Program
{
    public static void Main()
    {
        dynamic viewBag = new ExpandoObject();
        viewBag.ApiError = null;
        try
        {
            var result = (string?)viewBag.ApiError;
            Console.WriteLine("Cast successful: " + (result == null));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.GetType().Name + ": " + e.Message);
        }
    }
}
