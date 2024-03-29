using System.Text.RegularExpressions;
using VGManager.Library.Services.Interfaces;

namespace VGManager.Library.Services;

public class VariableFilterService : IVariableFilterService
{
    public IEnumerable<KeyValuePair<string, string>> Filter(IDictionary<string, string> variables, Regex regex)
    {
        return variables.Where(v => regex.IsMatch(v.Key.ToLower())).ToList();
    }

    public IEnumerable<KeyValuePair<string, string>> Filter(IDictionary<string, string> variables, string filter)
    {
        return variables.Where(v => filter.ToLower() == v.Key.ToLower()).ToList();
    }
}
