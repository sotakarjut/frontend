# frontend

Frontend project for the Sotakarjut larp communication application.

## Getting Started

* If you only want to test the last build, try http://iki.fi/dae/sotakarjut/
* Download and install Unity 3D version 2018.2.0f2 from https://unity3d.com/get-unity/download
* Download and open the project in Unity
* The scene SampleScene contains a Canvas object with the UI elements with the UI logic in script components. There are also some separate game objects with manager stubs that will communicate with the backend at some point in the future.

## Built With

* Unity 3d version 2018.2.0f2

## Production Deployment

* Merge any changes from the master into the production branch
* Build and test the app
* Create a zip file named Triton-frontend-webbuild.zip that has the build in a Triton-frontend-webbuild/ directory
* Create a new release to the production branch with a tag named as "vX.Y" where the X.Y is the version number, note that only releases with description are taken into account.
* This triggeris a webhook that will autodeploy the build on to production server

## Authors

* **Timo Kellomäki** - [Daemou](https://github.com/Daemou)
