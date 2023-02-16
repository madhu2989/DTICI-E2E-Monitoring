## Reset to Green Job

This job runs in a specified timeintervall and checks if there are elements with the state "Warning" or "Error" which needs to be reseted to the state "Ok". The decission whether an element needs to be resetet or not depends on the alerts which leed to the current state and on the "reset frequency" property of the check which belongs to the received alert. If the criteria matches the job generates a new alert message to change the state of the element to "Ok".

When creating a check the "reset frequency" can be added. The frequency needs to be in the timerange of 10 min to 1440 min.




