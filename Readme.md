## Installation Guide
 - Clone the repository
 - CD into the Fetch directory
 - run docker-compose up -d --build
 - Access the swagger via localhost:8080/swagger/index.html
 - Access the client via localhost:8081

## API Description
### Auth
 - POST /login/ Verifies a username/password combination against the database. Returns a token to be stored in the browser storage.
 - GET /verify/ Verifies that the token stored in the browser is valid. The allows a refresh without re-authenticating. This will return the role for this token that determines which view you see.
 - GET / Is a diagnostic (and unguarded endpoint - would be removed in a production system, but kept for troubleshooting any login credential issues) endpoint that returns the existing users.
### Diagnostic
 - GET /admin/stats/ An endpoint that returns the stats of the different API routes in the system. Returns total time spent executing and average response time as calculated by middleware.
### Request
 - POST /Request/New/ Used to create a new request. Via swagger, the ID is not required and won't be respected if passed. The client uses this to create new requests from the form.
 - GET /Request Returns all requests in the system
 - GET /Request/{id} Returns a specific request by id
 - DELETE /Request/{id} Deletes a specific request by id
 - POST /Request/{id}/commands/accept/{providerId} Specifies a request Id and provider Id who accepted. Triggers Kafka to process and update the status for all assocated providers.
 - POST /Request/{id}/commands/cancel Cancels a specific request. This updates all provider assocations to closed for this request.
 - GET /Request/providerView Returns the associated reuqests for a given provider. Determines provider affiliation using the token Username. 

## General flows

### Requestor
Username: testrequestor<br>
Password: testpass<br>

Using the client, enter this username/password combination to authenticate with the server. This will store a JWT token in your localhost:8081 local storage. <br><br>

Now, create a new request by completing the form and submitting. <br><br>

"Log out" by opening the browser store by inspecting the page, browsing to the app storage details, and deleting the JWT token. You could also access from a private browser since this shouldn't store beyond the instance of the page. <br><br>

### Provider
Provider A: <br>
Username: testproviderA <br>
Password: testpassA <br>

Is assigned to Dog Walking capability (id 0)<br><br>

Provider B: <br>
Username: testproviderB<br>
Password: testpassB<br>

Is assigned to Dog Walking and House Sitting (ids 0, 1)<br><br>

Provider C: <br>
Username: testproviderC<br>
Password: testpassC<br>

Is assigned to House Sitting (id 1)<br><br>

- Log into the client portal as a provider and view all associated requests. These requests are associated upon creation to each provider who "offers" this request type.
- You may view the details and "Accept" a request.
- This should accept your request and cancel all others. (NOTE: This has been shown, for some reason, to take a large amount of time to process on the server side. Sometimes this can take a few minutes, and I don't know why given that this topic and subscription chain are set up the same as the main request created chain. You can use the swagger to query the providerView or just refresh until the status changes.
