using System;
public class Program
{
    public static void Main()
    {
        dynamic viewBag = new System.Dynamic.ExpandoObject();
        try
        {
            var result = ((string)viewBag.ApiError) != null;
            Console.WriteLine("Success");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.GetType().Name + ": " + e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }
}
