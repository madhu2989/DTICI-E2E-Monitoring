# Export and Import API 

The Export/Import API of the Providence Service supports operations for exporting and importing whole environment trees.

## API Documentation
<code>
{{HOST}} = https://spp-monitoringservice-X.azurewebsites.net</code> where X stands for the environment the request is made to.

# Export Environment
This endpoint is accessible via: **GET: {{HOST}}/api/masterdata/environmentUpdate?environmentSubscriptionId={environmentSubscriptionId}**

The response of this endpoint call can be one of the following:
- **HttpStatusCode.OK** when the export was successful.
- **HttpStatusCode.BadRequest** when an invalid or no environmentSubscriptionId is provided.
- **HttpStatusCode.NotFound** when an unknown environmentSubscriptionId is provided.
- **HttpStatusCode.InternalServerError** when something went wrong during the export process.

# Import Environment
This endpoint is accessible via: **POST: {{HOST}}/api/masterdata/environmentUpdate?instance_name={{environmentname}}&environmentSubscriptionId={{environmentsubscriptionid}}&replace={Replace}**

Parameters:

- **instance_name** is a placeholder variable. All occurences of {instance_name} will be replaced with the values of these variable. 
- The **replace** parameter can have following values:
   -	**True**: Hereby all given elements in the payload will be replaces in the according environment. (If they don't exist they will be created)
   -	**False**: Hereby the elements in the environment will be supplemented with the elements in the payload.
   -	**All**: Hereby the elements in the enviroment will be oberriden by the elements given in the payload. 

The response of this endpoint call can be one of the following:
- **HttpStatusCode.OK** when the import was successful.
- **HttpStatusCode.BadRequest** when an invalid or no environmentSubscriptionId is provided.
- **HttpStatusCode.NotFound** when an unknown environmentSubscriptionId or environmentName is provided.
- **HttpStatusCode.InternalServerError** when something went wrong during the export process.

So for example it is possible to export the **dev** environment, replace in the response all appearances of "dev" with {environment_name}, and after that during the import again all occurences can be replaced by the right values with passing in the URL the value environment_name=test. With that mechanism all elements are imported with the correct envirnment name.

The payload of the import has following structure:
```
{
    "Services": [
        {
            "actions": [""],
            "elementId": "",
            "name": "",
            "description": ""
        }
    ],
    "Actions": [
        {
            "components": [""],
            "elementId": "",
            "name": "",
            "description": ""
        }
    ],
    "Components": [
        {
            "componentType": "",
            "elementId": "",
            "name": "",
            "description": ""
        },
        {
            "componentType": "",
            "elementId": "",
            "name": "",
            "description": ""
        }
    ],
    "Checks": [
        {
            "vstsLink": "",
            "frequency": -1,
            "elementId": "",
            "name": "",
            "description": ""
        }
    ]
}
```
