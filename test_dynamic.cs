using System;
using System.Dynamic;
public class Program
{
    public static void Main()
    {
        dynamic viewBag = new ExpandoObject();
        try
        {
            var x = (string)viewBag.DoesNotExist ?? "[]";
            Console.WriteLine("Success:" + x);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.GetType().Name + ": " + e.Message);
        }
    }
}
