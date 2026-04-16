A simple Local Job Scheduler.

This project is designed to capture the lifecycle of a process that we can launch locally.

The crux of this project is to distinguish clearly **stable and durable** data versus **transitory** data : 
  - Stable data is meant to be reused and **do not lose sense over time**. 
  - Transitory data is meant to support the execution and **can lose sense over time.**

For example, we can define a job to perform the task ' say hello '. 

This definition => **a job that does ' say hello '** is a stable data, the actual action can be performed today, or in 30 years but the relevance of the definition **a job that does ' say hello '** still stay the same. It's a definition.

In another hand, a transitory data can be a state i.e a job that does ' say hello ' when the **mood is happy**. The data **mood is happy** is transitory because it represents more of a guard, to prevent the job from being performed when the valid condition is not meet. 

Those two types of data should be stored somehow in our system, but they should not be managed in the same objet because **they do not keep the same value over time in our domain.**

!!! : A transitory data can be stored, and can still keep relevance over time BUT we have to better design our app and modelize the domain we want to, we have to design objects that stay true to their own nature as much as possible. 
We want to define **invariant** which have **to stay true in all valid states of our app** and build on top a reliable system which can rely on **transitiory data** to keep our system in a valid state.

A process is encapsulated in a domain object Job that we can manage in C# .NET environment.
A **Job** is basically a **definition** which encapsulates no state it only has **intrinsic value** :
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

It enables the use of better managed repositories for data persistence :
  - **JobDefinitionRepository** which is basically a store of all the Job the we can run using our system.
  - **JobRunRepository** which is a store for all occurences of our jobs and their final State.

Using this design, we create a stable and durable source of data.( i.e : that can be used after our Scheduler shuts down )
As for now, we don't really have a persistence layer, the reposity is meant to provide the mecanism to retrieve stable data but the actual persistence layer ( a DB or whatever )
isn't yet implemented.

In our system, a job is managed by a **LocalRunner** which only task is to start a job or cancel it **at a certain time**. It's not an orchestrator, it's not a service, but only an internal C# .NET tool meant to separate concern, we don't want jobs to be launched haphazardly.

This local runner furnishes the actual capabilities to **run** or **kill** the job and store it's reference with a relevant state in the appropriate repository.
It provides a **handle** to get a status of the actual running job and store it in a registry.

This design ensure that we have a composant only dedicated to monitor running jobs and transitory data pertaining to those running job during the whole app runtime.

On top of that, we have a **JobService** service layer, meant to expose the capabilities of our **LocalJobRunner** and our **Repositories**.
Using this API, we can get the relevant data without exposing much of the actual behaviour of our internal component and we enable composition to implement a much larger workflow.



 





