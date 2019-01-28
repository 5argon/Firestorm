# Firestorm 

Makeshift Cloud Firestore C# API that works on Unity via pure REST API. Contains only basic functions.

## Why Cloud Firestore

- Cloud Firestore is described as better than Readtime Database in every way, except that it is in beta and no Unity SDK yet.
- Decision to use Realtime Database or Firestore is a big forked path, since it affects the way you would design as hierarchy (Firestore) or flat with data duplications (RDB). There is probably 0% chance of easy migration. Unity devs will be faced with difficult decision of using RDB now and wait for SDK then having to overhaul design and migrate database, or just use Firestore with Firestorm while waiting for official SDK.
- The official C# Firestore API is available but Unity is not good with Nuget + it pulls in tons of dependencies that likely cause problem later. Firestorm puts all the work to `UnityWebRequest` to do REST call to ensure compatibility.

## Approach

- Use the currently available Unity Firebase SDK Auth to login before performing any Firestorm call.
- Firestorm will check on `FirebaseAuth.DefaultInstance.CurrentUser` and do `TokenAsync()`.
- The token will be an input to perform REST API call to Cloud Firestore.
- REST API performed by `UnityWebRequest`, which hopefully Unity will take care so it works with all platforms.
- There is nothing related to service account. I don't want to add external dependency to the FirebaseAdmin package.
- The Firestorm API is designed to roughly resemble C# Firestore API so that the transition to the real thing is not painful when it arrives.

## Requires

- Unity 2019.1 (may work with 2018.3 but I have enough time to care about backward compatibility sorry..)
- C# 7.3
- Firebase Unity SDK : Auth
- Newtonsoft.Json

## Why not Unity's JsonUtility

It sucks! The JSON from Firestore has polymorphic union fields (see [example](https://firebase.google.com/docs/firestore/reference/rest/v1beta1/Value)) and it is impossible to work with without at least JSON to `Dictionary` support to put the field names as dict key then do reflections etc.

## Not supported

- Type excluded in a document : Map, Geopoint (LatLng), bytes (use base-64 string instead), any mentioned types that is in an array.
- Transaction
- Batched write
- Ordering
- Limiting
- Listening for realtime updates
- Query cursor/pagination
- Offline data
- Managing index
- Import/export data

Let's wait for the Unity SDK for those. (They are already all supported in regular C# Firestore SDK)

## Available REST functions 

See here : https://firebase.google.com/docs/firestore/reference/rest/

exportDocuments	POST /v1beta1/{name=projects/*/databases/*}:exportDocuments 
importDocuments	POST /v1beta1/{name=projects/*/databases/*}:importDocuments 

batchGet	POST /v1beta1/{database=projects/*/databases/*}/documents:batchGet 
beginTransaction	POST /v1beta1/{database=projects/*/databases/*}/documents:beginTransaction 
commit	POST /v1beta1/{database=projects/*/databases/*}/documents:commit 
createDocument	POST /v1beta1/{parent=projects/*/databases/*/documents/**}/{collectionId} 
delete	DELETE /v1beta1/{name=projects/*/databases/*/documents/*/**} 
get	GET /v1beta1/{name=projects/*/databases/*/documents/*/**} 
list	GET /v1beta1/{parent=projects/*/databases/*/documents/*/**}/{collectionId} 
list	GET /v1beta1/{parent=projects/*/databases/*/documents/*/**}/{collectionId} 
patch	PATCH /v1beta1/{document.name=projects/*/databases/*/documents/*/**} 
rollback	POST /v1beta1/{database=projects/*/databases/*}/documents:rollback 
runQuery	POST /v1beta1/{parent=projects/*/databases/*/documents}:runQuery 
write	POST /v1beta1/{database=projects/*/databases/*}/documents:write 

create	POST /v1beta1/{parent=projects/*/databases/*}/indexes 
delete	DELETE /v1beta1/{name=projects/*/databases/*/indexes/*} 
get	GET /v1beta1/{name=projects/*/databases/*/indexes/*} 
list	GET /v1beta1/{parent=projects/*/databases/*}/indexes 

This repo is just enough to continue working on a game so yeah.. that's all I want in my game. 
(For example I didn't even implement delete. My game is not possible to delete data.)

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