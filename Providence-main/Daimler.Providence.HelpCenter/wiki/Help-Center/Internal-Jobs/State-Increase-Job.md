## State Increase Job

This job runs in a specified timeintervall and checks if there are elements with the state "Warning" which needs to be increased to the state "Error". The decission whether an element's state needs to be increased or not depends on the configured state increase rules of the system. If the criteria matches the job generates a new alert message to change the state of the element to "Error". In addition, all subsequent alerts that have the status "Warning" are automatically increased to alerts with the status "Error". 

## Screenshot with and without an active state increase rule.

|Without active state increase rule|With active state increase rule|
|--|--|
| ![State_Increase_rule.PNG](.attachments/State_Increase_rule-ae4d8b9d-9e15-4dc3-b6be-bc5156641e0d.PNG)   | ![SIR.PNG](.attachments/SIR-a49f22df-2c56-495e-9e15-b25d6ccb3116.PNG)   |

