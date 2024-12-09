## Installation Guide
 - Clone the repository
 - CD into the Fetch directory
 - run docker-compose up -d --build
 - Access the swagger via localhost:8080/swagger/index.html
 - Access the client via localhost:8081


## General flows

### Requestor
Username: testrequestor
Password: testpass

Using the client, enter this username/password combination to authenticate with the server. This will store a JWT token in your localhost:8081 local storage. 

Now, create a new request by completing the form and submitting. 

"Log out" by opening the browser store by inspecting the page, browsing to the app storage details, and deleting the JWT token. You could also access from a private browser since this shouldn't store beyond the instance of the page. 

### Provider
Provider A: 
Username: testproviderA
Password: testpassA

Is assigned to Dog Walking capability (id 0)

Provider B: 
Username: testproviderB
Password: testpassB

Is assigned to Dog Walking and House Sitting (ids 0, 1)

Provider C: 
Username: testproviderC
Password: testpassC

Is assigned to House Sitting (id 1)

- Log into the client portal as a provider and view all associated requests. These requests are associated upon creation to each provider who "offers" this request type.
- You may view the details and "Accept" a request.
- This should accept your request and cancel all others. (NOTE: This has been shown, for some reason, to take a large amount of time to process on the server side. Sometimes this can take a few minutes, and I don't know why given that this topic and subscription chain are set up the same as the main request created chain. You can use the swagger to query the providerView or just refresh until the status changes.
