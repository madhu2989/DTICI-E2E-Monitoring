# Tree and use cases

## Alerting strategy
_**You can’t fix what you don’t know is broken.**_

Alerting on what matters is critical, it is underpinned by collecting and measuring the right metrics and logs, as well a monitoring tool that is capable of storing, aggregating, visualizing, analyzing, and initiating an automated response when conditions are met. Improving observability of your services and applications can only be accomplished if you fully understand its composition in order to map/translate that into a detailed monitoring configuration to be applied by the monitoring platform, including the predictable failure states (the symptoms not the cause of the failure) that make sense to alert on. 

***
## 1. Alert for Checks (without AlertName)

![Alert_Dynamic_Check_new.jpg](.attachments/Alert_Dynamic_Check_new-2285c5f4-d505-4f59-9bc9-693683bb4975.jpg)
***
## For Example Dynamic Check:
![Alert_Dynamic_Check_detail_new.jpg](.attachments/Alert_Dynamic_Check_detail_new-97bedf9c-1225-41ad-b57d-5d6615f1e36f.jpg)

### Alert

|Attribute|Value|
|:--|:--:|
| CheckId     |  Watchdog |
| ElementId  |  Component1|
| State          |  Error |

### Procedure in background
- search for node with ElementId
- attach Check to the node if not existant, else update
- set the state of the Check
- propagate the state transition towards the root of the tree

### Statetransitions

| CheckId    |    ElementId  |    AlertName   |  State   |Type |
|:--:|:--|:--:|:---|:--|
|Watchdog|  Component1  | - | Error   | Component |
|-    |  Action1| - | Error | Action |
|-    |  Service1| - | Error | Service |
|Watchdog|  Component1  | - | Error   | Component |
|-    |  Action2| - | Error | Action |
|-    |  Service2| - | Error | Service |
|-    |  DevB2B| - | Error | Environment |

***

## 2. Alert for Checks (with AlertName)

![Alert_Metric_Check_new.jpg](.attachments/Alert_Metric_Check_new-8959a1f2-dc23-4dca-8bae-8843c772daa3.jpg)
***
## For Example Metric Check:
![Alert_Metric_Check_detail_new.jpg](.attachments/Alert_Metric_Check_detail_new-e08f2a50-4b9b-43ca-aaf6-4e6e9f46e1e9.jpg)

### Alert

|Attribute|Value 1|Value 2|
|:--|:--:|:--:|
| CheckId     |  MetricAlert |MetricAlert |
| ElementId  |  Component3|Component4|
| State|  Error |Error|
| AlertName | CPU |MEM|

### Procedure
- search for node with ElementId
- attach Check to the node if not existant, else update
- set the state of the Check
- propagate the state transition towards the root of the tree
- Attach Check for AlertName

### Statetransitions

|CheckId    |    ElementId  |    AlertName   |  State   |Type|
|:--:|:--|:--:|:--|:--|
|metricalert|  Component3| CPU| Error | Component |
|metricalert|  Component3| MEM| Error | Component  |
|-|  Component3| - | Error | Component | 
|metricalert|  Component4| CPU| Error | Component |
|metricalert|  Component4| MEM| Error | Component  |
|-|  Component4| - | Error | Component  | 
|-|  Action2| -| Error | Action |
|-    |  Service2| - | Error | Service |
|-    |  DevB2B            | - | Error | Environment |

