A simple Local Job Scheduler.

This project is designed to capture the lifecycle of a process that we can launch locally.
A process is encapsulated in a domain object Job that we can manage in C# .NET environment.
A **Job** is basically a **definition** which encapsulates no state it only as **intrinseque value** :
  - Name
  - CommandLine
  - MaxRetryCount ( basically its policy when runned )

A **Job Run** is the **occurence**, references a job definition to be launched, and store the **actual state** of the occurence.
It can be :
  - Pending
  - Running
  - Cancelled
  - Faulted
  - Succeeded


This simple separation enables a more reliable design upon **asynchronous call** and a simple **State Machine**  that we can enrich as needed.
It enables the use of better managed repositories for data persistence :
  - **JobDefinitionRepository** which is basically a store of all the Job the we can run using our system.
  - **JobRunRepository** which is a store for all occurences of our jobs.

Using this design, we create a stable and durable source of data.( i.e : that can be used after our Scheduler shuts down )
As for now, we don't really have a persistence layer, the reposity is meant to provided the mecanism to retrieve stable data but the actual persistence layer ( a DB or whatever )
isn't yet implemented. 
We can run a Job using their definition retrieved using our repository and retrieve data related to the actual occurence of the run.





