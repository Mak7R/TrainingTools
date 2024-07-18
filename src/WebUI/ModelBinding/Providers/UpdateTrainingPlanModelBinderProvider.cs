using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebUI.ModelBinding.ModelBinders;
using WebUI.Models.TrainingPlan;

namespace WebUI.ModelBinding.Providers;

public class UpdateTrainingPlanModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType == typeof(UpdateTrainingPlanModel))
        {
            return new UpdateTrainingPlanModelBinderAttribute();
        }

        return null;
    }
}