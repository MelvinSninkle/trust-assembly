# Markdown File



##Goal

Read a set of news sites and collect their headlines.

The data will be stored TBD

Then other parts of the system will read and acton the headine data.

First: just world religious news, since it's odd
Then : a simpler site like NYT
Next: Fox news to get some balance

... then see what tech works best to get the most sites
 probably playwright + caching
 
 TODO: choose a place to put the data- AWS Dynamo?
 TODO: choose timing - when to run, when to refresh data
 TODO: error handling. who needs to be notified and how?

 TODO: it's console now, but we can pretty easily make this a service that runs on a schedule


 TODO:  make it so that it will be straightforward to add more sites with just a few settings and overrides for each one

