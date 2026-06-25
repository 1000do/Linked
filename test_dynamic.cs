using System;
public class Program
{
    public static void Main()
    {
        dynamic viewBag = new System.Dynamic.ExpandoObject();
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
