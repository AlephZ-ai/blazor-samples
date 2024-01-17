// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Microsoft.TypeChat.Schema;

namespace Microsoft.TypeChat;

/// <summary>
/// Program translator that translates user requests into programs that call APIs defined by Microsoft Semantic Kernel Plugins
/// </summary>
public class PluginProgramTranslator
{
    ProgramTranslator _translator;
    PluginApi _pluginApi;
    SchemaText _pluginSchema;

    /// <summary>
    /// Create a new translator that will produce programs that can call all skills and
    /// plugins registered with the given semantic kernel
    /// </summary>
    /// <param name="kernel"></param>
    /// <param name="model"></param>
    public PluginProgramTranslator(IKernel kernel, ILanguageModel model)
    {
        _pluginApi = new PluginApi(kernel);
        _pluginSchema = _pluginApi.TypeInfo.ExportSchema(_pluginApi.TypeName);
        _translator = new ProgramTranslator(
            model,
            new ProgramValidator(new PluginProgramValidator(_pluginApi.TypeInfo)),
            _pluginSchema
        );
    }

    /// <summary>
    /// Translator being used
    /// </summary>
    public ProgramTranslator Translator => _translator;

    /// <summary>
    /// The "API" formed by the various plugins registered with the kernel 
    /// </summary>
    public PluginApi Api => _pluginApi;

    public SchemaText Schema => _pluginSchema;

    public Task<Program> TranslateAsync(string input, CancellationToken cancelToken = default)
    {
        return _translator.TranslateAsync(input, cancelToken);
    }
}
