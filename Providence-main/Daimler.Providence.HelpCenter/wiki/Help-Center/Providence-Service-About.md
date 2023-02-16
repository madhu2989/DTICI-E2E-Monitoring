# About

#### Providence Service Summary
The providence service is a custom monitoring solution built specifically to monitor multiple services.
The main purpose of the monitoring service is to provide a single source of information of the platforms current health state. The health state of the platform is calculated bottom-up from the state of the services (B2B-Services and Basic-Services) and their respective components.
The monitoring service contains a structure of the platform: Services are made of actions, which on the other hand are built out of components. Components can be Azure Resources or external services.

Services are green/OK unless an alert on one of their dependencies is triggered, which will then turn their state to WARNING or ERROR.

#### Alerts
Alerts are defined in the platform and when triggered, they change the state of a service's health. The following table shows each state and its implication.

|Component's state|Implication|Example|
|--|--|--|
|OK|No issues reported/ alerts triggered|-|
|WARNING|Operations team action required soon to prevent outage of the service|CPU load running continuously at > 80% |
|ERROR|Action required because a component is failing and auto-recovery was unsuccessful|Watchdog check for microservice failing, Data Factory Pipeline run failing|

#### No Current Data available message
To ensure that the monitoring service is receiving the newest updates of the platform, heartbeat signals are sent periodically from each environment to the service. If no new heartbeats arrive, the connection with the backend might be lost. In this case, data shown in the dashboard might not reflect the current health state of the environments. These are marked with "No current data available".
On the landing page, for each environment the last received heartbeat is shown in the environments tile.

![Items.png](.attachments/Items-70e7bc10-8749-462f-af90-bb477dc7350c.png)
