## Email Notifications Job
This job runs in a specified timeintervall and checks if the states of an element matches the configuration of any notification rules. If so the job notifies the users mentioned in the rule via an email that a specific event occured.   

The emails contains the following information:


- Element (which element is affected)
  - Environment Name 	
  - Environment ID 	
  - Notification Level 
  - Element ID 	
  - New State 	
  - URL to the Environment	

- Alert (Which alert is responsible for the email)
  - State 	
  - Timestamp 	
  - Check ID 	
  - Affected component 	
  - Record ID 	
  - Alert Description 	