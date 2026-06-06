using System.Text.RegularExpressions;

namespace CourseMarketplaceBE.Share.Helpers;

public static class CaseMapper
{
    public static string ToSnakeCase(this string input) =>
        string.IsNullOrEmpty(input) ? input : 
        Regex.Replace(input, "(?<!^)([A-Z])", "_$1").ToLower();

    public static string ToPascalCase(this string input){
        if (string.IsNullOrEmpty(input)) return input;
        
        var words = input.Split('_');
        var result = "";
        for(int i = 0; i< words.Length; i++){
            if (string.IsNullOrEmpty(words[i])) continue;
            var word = char.ToUpper(words[i][0]) + words[i][1..].ToLower();
            result += word;
        }
        return result;
    }
}
    
