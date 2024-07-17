using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using WebUI.ModelBinding.CustomModelBinders;
using WebUI.Models.TrainingPlan;

namespace WebUI.ModelBinding.CustomModelBindingProviders;

public class UpdateTrainingPlanModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType == typeof(UpdateTrainingPlanModel))
        {
            return new UpdateTrainingPlanModelBinder();
        }

        return null;
    }
}