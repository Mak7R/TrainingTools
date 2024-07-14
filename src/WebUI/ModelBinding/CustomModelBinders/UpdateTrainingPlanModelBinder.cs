﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using WebUI.Models.TrainingPlanModels;

namespace WebUI.ModelBinding.CustomModelBinders;

public class UpdateTrainingPlanModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        // Ensure the request has form content type
        var request = bindingContext.HttpContext.Request;
        if (!request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        // Read the request body
        using (var reader = new StreamReader(request.Body))
        {
            var json = await reader.ReadToEndAsync();

            try
            {
                // Deserialize JSON to UpdateTrainingPlanModel
                var model = JsonConvert.DeserializeObject<UpdateTrainingPlanModel>(json);

                // Set the model binding result
                bindingContext.Result = ModelBindingResult.Success(model);
            }
            catch (JsonException)
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }
    }
}