Most of the desing was as I described in the Project 3 OCD. Some differences are:

- Parsing and test setup is done in the child thread but not in the child app domain.
  A simple syncronization is used to handle multiple clients with multiple waiting stages.
  A dictionary of locks is monitored to resume execution of tests after receiving required
  files from repository.

- XML file is parsed only once in test preparation stage. Some simplified serialized data is
  passed to child app domain instead of whole xml description.

- Server is used by Test Harness instead of having it a seperate thread.