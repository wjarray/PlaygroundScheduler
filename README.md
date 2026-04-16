A simple Local Job Scheduler.

This project is designed to capture the lifecycle of a process that we can launch locally.

The crux of this project is to distinguish clearly **stable and durable** data versus **transitory** data : 
  - Stable data is meant to be reused and **do not lose sense over time**. 
  - Transitory data is meant to support the execution and **can lose sense over time.**

For example, we can define a job to perform the task ' say hello '. 

This definition => **a job that does ' say hello '** is a stable data, the actual action can be performed today, or in 30 years but the relevance of the definition **a job that does ' say hello '** still stay the same. It's a definition.

In another hand, a transitory data can be a state i.e a job that does ' say hello ' when the **mood is happy**. The data **mood is happy** is transitory because it represents more of a guard, to prevent the job from being performed when the valid condition is not meet. 

Those two types of data should be stored somehow in our system, but they should not be managed in the same objet because **they do not keep the same value over time in our domain.**


A process is encapsulated in a domain object Job that we can manage in C# .NET environment.
A **Job** is basically a **definition** which encapsulates no state it only as **intrinsic value** :
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


This simple separation enables a more reliable design upon **asynchronous call** and a simple **State Machine** that we can enrich as needed.
Depending on data a



It enables the use of better managed repositories for data persistence :
  - **JobDefinitionRepository** which is basically a store of all the Job the we can run using our system.
  - **JobRunRepository** which is a store for all occurences of our jobs and their final State.

Using this design, we create a stable and durable source of data.( i.e : that can be used after our Scheduler shuts down )
As for now, we don't really have a persistence layer, the reposity is meant to provide the mecanism to retrieve stable data but the actual persistence layer ( a DB or whatever )
isn't yet implemented. 
We can run a Job using their definition retrieved using our repository and retrieve data related to the actual occurence of the run.

Upon runtime, the Scheduler make uses of a registry to monitor the actual activities of runs. 
The registry acts the same way as a store but only for currently running tasks and is updated on the fly.

 





