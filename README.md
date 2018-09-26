# frontend

Frontend project for the Sotakarjut larp communication application.

## Configuring terminal name

* In a windows build the terminal name can be configured from "Sotakarjut frontend_Data/StreamingAssets/config.json".

## Getting Started

* Download and install Unity 3D version 2018.2.0f2 from https://unity3d.com/get-unity/download
* Download and open the project in Unity
* The scene SampleScene contains a Canvas object with the UI elements with the UI logic in script components. There are also some separate game objects with managers that communicate with the backend.

## Built With

* Unity 3d version 2018.2.0f2

## Production Deployment

* Merge any changes from the master into the production branch
* Build and test the app
* Create a zip file named Triton-frontend-webbuild.zip that has the build in a Triton-frontend-webbuild/ directory
* Create a new release to the production branch with a tag named as "vX.Y" where the X.Y (e.g. v1.0) is the version number. Note that only releases with description are taken into account, so be sure to add the description to the release.
* To trigger the auto-deployment, update the RELEASE file with the latest version number and commit+push it to the production branch
* This triggers a webhook that will autodeploy the build on the production server

## Authors

* **Timo Kellomäki** - [Daemou](https://github.com/Daemou)
