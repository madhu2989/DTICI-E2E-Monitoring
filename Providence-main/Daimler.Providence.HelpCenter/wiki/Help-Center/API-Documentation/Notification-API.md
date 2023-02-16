# Notification API

The Notification API of the Providence Service supports operations for creating, updating and deleting notification rules for specific users and element states. 

## API Documentation

<code>
{{HOST}} = https://spp-monitoringservice-X.azurewebsites.net</code> where X stands for the environment the request is made to.



### Create Notification Rule

This endpoint is accessible via: **POST: {{HOST}}/api/notificationRules**

Following payload is used to create a notification rule.

**Notice:**
- The **levels** list shows for which kind of elements you want to be notified (To avoid spam this always notifies you about the highest state of this list)
  So for example if a **Component** goes to state **ERROR** and therefore also an **Action** goes to **ERROR** you receive a notification for the higher element (the Action) only.
- The **states** list show for which kind of states you want to be notified
- the **notificationInterval** shows after which period of time (in seconds) you want to be notified about a state change

```json
{
    "environmentSubscriptionId": "XYZ",
    "levels": ["Environment", "Service", "Action", "Component"],
    "emailAddresses": "NotifyMeUser@daimler.com",
    "states": ["Error","Warning"],
    "notificationInterval": 120,
    "isActive": true
}
```

The response of this endpoint call can be one of the following:
- **HttpStatusCode.Created** when the notification rule was created successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the created notification rule or the environment with the specified "environmentSubscriptionId" was not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the creation process.

### Update Notification Rule

This endpoint is accesible via: **PUT: {{HOST}}/api/notificationRules/{id}**

Following payload is used to update an existing notification rules (it is the same as when creating a new notification rule)

```json
{
    "environmentSubscriptionId": "XYZ",
    "levels": ["Environment", "Service", "Action", "Component"],
    "emailAddresses": "NotifyMeUser@daimler.com",
    "states": ["Error","Warning"],
    "notificationInterval": 120,
    "isActive": true
}
```
The response of this endpoint call can be one of the following:
- **HttpStatusCode.NoContent** when the notification rule was updated successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the created notification rule or the environment with the specified "environmentSubscriptionId" was not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the update process.

### Delete Notification Rule

This endpoint is accessible via: **DELETE: {{HOST}}/api/notificationRules/{id}**

The response of this endpoint call can be one of the following:
- **HttpStatusCode.Accepted** when the notification rule was deleted successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the notification rule to delete was not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the deletion process.

### Get All Notification Rules

This endpoint is accesible via: **GET: {{HOST}}/api/notificationRules**

The response of this endpoint call can be one of the following:
- **HttpStatusCode.Ok** when the searched notification rules were retrieved successfully.
- **HttpStatusCode.InternalServerError** when something went wrong during the get process.

### Get Notification Rule by Id

This endpoint is accessible via: **GET: {{HOST}}/api/notificationRules/{id}**

The response of this endpoint call can be one of the following:
- **HttpStatusCode.Ok** when the searched notification rule  were retrieved successfully.
- **HttpStatusCode.BadRequest** when the payload/url contains invalid parameter.
- **HttpStatusCode.NotFound** when the  searched notification rule was not found in the database.
- **HttpStatusCode.InternalServerError** when something went wrong during the get process.


## Notification Logic

After the API is clear this section should explain how the notification rules work.

There are 3 options how a user can configure a notification rule:
- User wants to be notified about **WARNING** and **ERROR** states [1]
- User wants only to be notified about **WARNING** states [2]
- User wants only to be notified about **ERROR** states [3]

Depending on the rule configuration following scenarios may occur:
**NOTICE:** 
- **"Warning / Error occured"** notifications are only send if the state of the specified element lasts longer than the **notificationInterval** configured in the rule
- **"Warning / Error resolved"** notifications are only send if a previous Warning / Error notification was send (to avoid spam)
- **"Warning / Error resolved"** notifications are send immediately and do not depend on the **notificationInterval** of a rule

### State Change: OK -> WARNING
- [1] User receives a **"Warning occured: ..."** notification
- [2] User receives a **"Warning occured: ..."** notification
- [3] User receives no notification

### State Change: OK -> ERROR
- [1] User receives a **"ERROR occured: ..."** notification
- [2] User receives no notification
- [3] User receives a **"ERROR occured: ..."** notification

### State Change: WARNING -> ERROR
- [1] User receives a **"ERROR occured: ..."** notification
- [2] User receives no notification
- [3] User receives a **"ERROR occured: ..."** notification

### State Change: WARNING -> ERROR -> WARNING
- [1] User receives a **"ERROR resolved: ..."** notification
- [2] User receives no notification
- [3] User receives a **"ERROR resolved: ..."** notification

### State Change: OK -> ERROR -> WARNING
- [1] User receives a **"ERROR resolved: ..."** notification
- [2] User receives a **"Warning occured: ..."** notification
- [3] User receives a **"ERROR resolved: ..."** notification

### State Change: ERROR -> OK
- [1] User receives a **"ERROR resolved: ..."** notification
- [2] User receives no notification
- [3] User receives a **"ERROR resolved: ..."** notification

### State Change: WARNING-> OK
- [1] User receives a **"WARNING resolved: ..."** notification
- [2] User receives a **"WARNING resolved: ..."** notification
- [3] User receives no notification
