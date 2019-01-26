# Firestorm 

Adopt Cloud Firestore early!

## Why

- Cloud Firestore is described as better than Readtime Database in every way, except that it is in beta and no Unity SDK yet.
- Decision to use Realtime Database or Firestore is a big forked path, since it affects the way you would design as hierarchy (Firestore) or flat with data duplications (RDB). There is probably 0% chance of easy migration. Unity devs will be faced with difficult decision of using RDB now and wait for SDK then having to overhaul design and migrate database, or just use Firestore with Firestorm while waiting for official SDK.
- The Unity SDK will likely be a packed DLL like it currently is at present. This had given me multiple headache where I could not fix the problem by myself without access to source code especially when upgrading to beta/alpha Unity build. Firestorm will be completely independent from Firebase Unity SDK, including the auth part before it can use Firestore.
- It does not cover anything other than Auth and Firestore. If you want to do Auth to Storage/Cloud Functions/etc. please just use the Unity SDK to auth again separately.
- Because it maintains only compatibility of Auth and Firestore maybe it is easier and faster to adopt a new Firestore feature added by Google.

## Approach

- Request service account credential with [Google.Apis.Auth](https://www.nuget.org/packages/Google.Apis.Auth/)
- Or request user's credential by REST on the Firebase Auth.
- Use the C# Nuget package [Google.Apis.Firestore](https://www.nuget.org/packages/Google.Apis.Firestore.v1beta2/)
- Use either credential for REST on the Firebase Cloud Firestore.
- REST is bridged to you via `UnityWebRequest` so that the REST request works on all platform as Unity promised **hopefully**.
- Wrap all `UnityWebRequest` in a better C# `async` methods that feels nice to use.


## Requires
- Unity 2019.1 (may work with 2018.3 but I have enough time to care about backward compatibility sorry..)
- C# 7.3

### Needed in your Plugins folder

To get all related Nuget packages do :


I will not include external `dll` in this repo, since you might already have them to work with other code in your Unity project. It will conflict easily if this repo was to be pulled into Unity as a package.

- Google.Apis
- Google.Apis.Auth
- Google.Apis.Core
- Google.Apis.Firestore.v1beta2
- Newtonsoft.Json

## Test

Both unit test (run in editor) and play mode test (able to build and run on the device) are included. They will use internet and a Cloud Firestore database you have to setup for the test to work on and it will cost your bill! It will test using both service account and faked user account. (Which is registered using service account)

## "Oh no REST sucks, why don't you use gRPC?"

In short I gave up, but it looks like a better than REST way if done right. It is just too messy with Unity. (In normal C# where NUGET is usable I would do RPC way.)

### What is it

It lets you do RPC with generated code, so it feels like you are calling regular function and it magically do remote calls. The code is generated from Protobuf file. The files in GRPC folder was grabbed from generated C# files from [Google API repo](https://github.com/googleapis/googleapis).

Therefore the code to talk with Firestore in Firestorm will feel just kinda like we already have Unity SDK, because the generated gRPC codes are in C# already. (Not by doing REST to a URL, etc.)

### What is the problem

Basically the "unloading assembly because it could cause crash in runtime" error message. I add and add all requirement by Nuget chain but finally arrives at the point where I don't know what is the cause of that anymore.

### Some pointers if you want to try doing it gRPC way

First install gRPC stuff, there is a beta `unitypackage` by Google too. See [here](https://packages.grpc.io/archive/2019/01/f7a4d1e0c74f3c76bd09d8f54ab1d2c357df2788-6affcdc9-9f89-475b-817b-14263e865b8e/index.xml) for example you can see **grpc_unity_package.1.19.0-dev.zip**. Then install gRPC csharp plugin from somewhere, Google it and you should found it. It allows `protoc` to generate client stub methods when it see `service` syntax in the `.proto` file.

(At the time when you are reading this things might changed already.) Use `artman` with things in the `googleapis` repo. Go to [here](https://googleapis-artman.readthedocs.io/en/latest/installing.html) and follow it. You will be installing `pipsi` and starting `docker` daemon before you can use `artman`, then you will have to download Docker image of Google's `artman` by following the terminal. Note that all the things surrounding gRPC and `googleapis` seems to be sparsely documented than usual.

Finally you will be running something like : 

```
~/.local/bin/artman --config google/firestore/artman_firestore.yaml generate csharp_gapic
```

The `yaml` file would be updated/changed in the future? I don't know..

You will now notice that the `artman` does not include the `Firestore.Admin` section, so you cannot do gRPC with admin API. Also it is missing some more references, you will have to install more Nuget package such as [CommonProtos](https://www.nuget.org/packages/Google.Api.CommonProtos/). And in an hours or two maybe you will arrive with the same "unloading assembly" error as me?

## How about Google.Cloud.Firestore

When I do 

```
nuget install google.cloud.firestore -Prerelease
```

I got tons of related Nuget which in turn resolves into gRPC again. I think it is scary and difficult to get it working (at runtime too) so I didn't continue this path either.