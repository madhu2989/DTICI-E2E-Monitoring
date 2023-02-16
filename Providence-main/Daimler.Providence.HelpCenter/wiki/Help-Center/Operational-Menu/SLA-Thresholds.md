# SLA Thresholds

The system monitoring works by "observing" the individual instances of an environment and "tracking" the various metrics within that instance. Each metric has two defined thresholds, one for minor (warnings) and one for major or critical problems (errors).

If a metric is above the error threshold it will be seen as error state. 
If a metric is above the warning threshold but under the error threshold it will be seen as warning state

The thresholds are administered by the Administrator and can be visualized in the SLA report.

Hint: Define thresholds which reflect the expectations of your customers.

**Warning:** The experience of the enduser can be influenced badly without running in critical issues. With a warning you can indicate to fix things before your system is running in crictical circumstances.

**Error:** If you are in the critical state then you need to fix issues immediately to come back to normal business again. 

With administrating the thresholds you define following options:

- **Environment Name:** 
Choose the environment to administrate.
- **Source:** 
Choose between Option "Error" and "Error + Warning". This specifies whether alerts with the status "Warning" are to be taken into account when calculating the service downtimes.
- **Threshold Warning:** 
Define the threshold for the warning in %, this values needs to be higher then the error threshold.
- **Threshold Error:** 
Define the threshold for the state error in %

![SLA_Threshold.png](.attachments/SLA_Threshold-a82cdef8-1c31-4484-b3cf-0bbcea5d9e15.png)
