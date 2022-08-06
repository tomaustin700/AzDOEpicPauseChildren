# AzDOEpicPauseChildren
This is a basic Azure Function that accepts a Azure DevOps work item updated webhook body and will set the state to of all children (and childrens children) to state new. The function is written using .NET 6 and is using Function worker runtime 4.0.

## Architecture 
This Azure Function recieves data from Azure DevOps via a work item updated webhook configured within Azure DevOps, it then uses a [PAT](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=Windows) to authenticate back to Azure DevOps to modify work items. The PAT is stored within Azure Keyvault and retrieved at function execution time.


![Azure resources](./Documentation/azsetup.png)

## Getting Started
The first thing to do is create all the Azure resources required, this is simply a keyvault and an Azure function. Creating the function will also create an associated storage account and consumption service plan, you wont need to do anything with these but they are required for the function to run.

### Acquiring a PAT
You will need to generate a PAT from Azure DevOps so the function can authenticate correctly, to do this press the User Settings button in the top right of Azure DevOps and select 'Personal access tokens'

![PAT button](./Documentation/PATbutton.png)

Create a new token and grant it read, write & manage permissions - no other permissions are required.

![PAT settings](./Documentation/patsettings.png)

Make a note of the generated PAT. It will only be shown once and we need to add it to the Azure Keyvault you created shortly.

### Azure Resource Configuration
There are a few bits of configuration that need to be done on the Azure Function and within the Keyvault. Firstly open your function, select 'Identity' on the left and turn on System Assigned identity. This will allow us to grant the function permission to access the keyvault.

![func identity](./Documentation/funcidentity.png)

We also need to set two application settings on the Function, one to pass it our Azure DevOps instance url and another to give it the Keyvault URL. Add a new application setting and call it 'AzDOBaseURL', set the value to the URL of your Azure DevOps instance.

![base url](./Documentation/baseurl.png)

Add another setting called 'KeyVaultUri' and set it to the Uri of your Azure Keyvault

![key vault uri](./Documentation/keyvaulturi.png)

Once you've added both the settings make sure you press 'Save' at the top of the Application Settings page.

Now navigate to your Key vault and select 'Secrets' on the left

![secrets](./Documentation/secretsbutton.png)

Press the 'Generate/Import' button at the top and add a new Manual secret called 'azure-devops-pat', set its value to the PAT you acquired earlier

![secrets](./Documentation/patkv.png)

Once created press 'Access Policies' on the left and press the '+ Add Access Policy' button.

Grant 'Secret Permissions' Get and then press the 'None selected' hyperlink next to 'Service Principal', type the name of your Function and select it.

![policy](./Documentation/kvaccesspolicy.png)

That's all the Azure configuration done, before configuring Azure DevOps to send webhook events to the function we need to retrieve a Functions key to allow Azure DevOps to authenticate with the function. Head back to your function and select 'App keys'. Note down the value of the default key.

![func key](./Documentation/funckey.png)

### Azure DevOps Configuration

Now we just need to configure Azure DevOps. Head to DevOps and select the Project Settings button in the button left, once there select 'Service Hooks'

![service hooks](./Documentation/hooks.png)

Press the 'Create subscription' button and select 'Web Hooks' and then press 'Next'

![web hooks](./Documentation/wh.png)

Change the trigger to 'Work Item Updated', set the 'Work Item Type' to Epic and the 'Field' to State

![trigger](./Documentation/trigger.png)

Set the URL to the URl of your Azure function followed by /api/PauseEpicChildren, inside the Headers box type 'x-function-key:defaultfunctionkeyfromearlier'

![settings](./Documentation/whsettings.png)

Now press 'Finish'. Now all you need to do it deploy the Function and everything should work. When a Epic is set to 'New' all of it's children and all of their children will also be set back to 'New'.
