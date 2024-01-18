using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using System.Text.RegularExpressions;
using VGManager.Library.Models.Models;
using VGManager.Library.Models.StatusEnums;
using VGManager.Library.Services.Models.VariableGroups.Requests;
using VGManager.Library.Services.Models.VariableGroups.Results;

namespace VGManager.Library.Services;

public partial class VariableService
{
    public async Task<AdapterResponseModel<IEnumerable<VariableResult>>> GetVariablesAsync(
        VariableGroupModel variableGroupModel,
        CancellationToken cancellationToken = default
        )
    {
        var vgEntity = await _variableGroupConnectionRepository.GetAllAsync(cancellationToken);
        var status = vgEntity.Status;

        if (status == AdapterStatus.Success)
        {
            return GetVariablesAsync(variableGroupModel, vgEntity, status);
        }
        else
        {
            return new()
            {
                Status = status,
                Data = new List<VariableResult>(),
            };
        }
    }

    private AdapterResponseModel<IEnumerable<VariableResult>> GetVariablesAsync(
        VariableGroupModel variableGroupModel,
        AdapterResponseModel<IEnumerable<VariableGroup>> vgEntity,
        AdapterStatus status
        )
    {
        var matchedVariables = new List<VariableResult>();
        var filteredVariableGroups = variableGroupModel.ContainsSecrets ?
                        _variableFilterService.Filter(vgEntity.Data, variableGroupModel.VariableGroupFilter) :
                        _variableFilterService.FilterWithoutSecrets(true, variableGroupModel.VariableGroupFilter, vgEntity.Data);

        var valueFilter = variableGroupModel.ValueFilter;
        var keyFilter = variableGroupModel.KeyFilter;
        Regex? valueRegex = null;

        if (valueFilter is not null)
        {
            try
            {
                valueRegex = new Regex(valueFilter.ToLower(), RegexOptions.None, TimeSpan.FromMilliseconds(5));
            }
            catch (RegexParseException ex)
            {
                _logger.LogError(ex, "Couldn't parse and create regex. Value: {value}.", valueFilter);
                return new()
                {
                    Status = status,
                    Data = matchedVariables,
                };
            }
        }

        if (variableGroupModel.KeyIsRegex ?? false)
        {
            Regex keyRegex;
            try
            {
                keyRegex = new Regex(keyFilter.ToLower(), RegexOptions.None, TimeSpan.FromMilliseconds(5));
            }
            catch (RegexParseException ex)
            {
                _logger.LogError(ex, "Couldn't parse and create regex. Value: {value}.", keyFilter);
                return new()
                {
                    Status = status,
                    Data = matchedVariables,
                };
            }

            foreach (var filteredVariableGroup in filteredVariableGroups)
            {
                matchedVariables.AddRange(
                    GetVariables(keyRegex, valueRegex, filteredVariableGroup)
                    );
            }
        }
        else
        {
            foreach (var filteredVariableGroup in filteredVariableGroups)
            {
                matchedVariables.AddRange(
                    GetVariables(keyFilter, valueRegex, filteredVariableGroup)
                    );
            }
        }

        return new()
        {
            Status = status,
            Data = matchedVariables,
        };
    }

    private IEnumerable<VariableResult> GetVariables(
        string keyFilter,
        Regex? valueRegex,
        VariableGroup filteredVariableGroup
        )
    {
        var filteredVariables = _variableFilterService.Filter(filteredVariableGroup.Variables, keyFilter);
        return CollectVariables(valueRegex, filteredVariableGroup, filteredVariables);
    }

    private IEnumerable<VariableResult> GetVariables(
        Regex keyRegex,
        Regex? valueRegex,
        VariableGroup filteredVariableGroup
        )
    {
        var filteredVariables = _variableFilterService.Filter(filteredVariableGroup.Variables, keyRegex);
        return CollectVariables(valueRegex, filteredVariableGroup, filteredVariables);
    }

    private IEnumerable<VariableResult> CollectVariables(
        Regex? valueRegex,
        VariableGroup filteredVariableGroup,
        IEnumerable<KeyValuePair<string, VariableValue>> filteredVariables
        )
    {
        var result = new List<VariableResult>();
        foreach (var filteredVariable in filteredVariables)
        {
            var variableValue = filteredVariable.Value.Value ?? string.Empty;
            if (valueRegex is not null)
            {
                if (valueRegex.IsMatch(variableValue.ToLower()))
                {
                    result.AddRange(
                        AddVariableResult(filteredVariableGroup, filteredVariable, variableValue)
                        );
                }
            }
            else
            {
                result.AddRange(
                    AddVariableResult(filteredVariableGroup, filteredVariable, variableValue)
                    );
            }
        }
        return result;
    }

    private IEnumerable<VariableResult> AddVariableResult(
        VariableGroup filteredVariableGroup,
        KeyValuePair<string, VariableValue> filteredVariable,
        string variableValue
        )
    {
        var subResult = new List<VariableResult>();
        if (filteredVariableGroup.Type == SecretVGType)
        {
            var azProviderData = filteredVariableGroup.ProviderData as AzureKeyVaultVariableGroupProviderData;
            subResult.Add(new VariableResult()
            {
                Project = _project ?? string.Empty,
                SecretVariableGroup = true,
                VariableGroupName = filteredVariableGroup.Name,
                VariableGroupKey = filteredVariable.Key,
                KeyVaultName = azProviderData?.Vault ?? string.Empty
            });
        }
        else
        {
            subResult.Add(new VariableResult()
            {
                Project = _project ?? string.Empty,
                SecretVariableGroup = false,
                VariableGroupName = filteredVariableGroup.Name,
                VariableGroupKey = filteredVariable.Key,
                VariableGroupValue = variableValue
            });
        }
        return subResult;
    }
}