# Firestorm 

Adopt Cloud Firestore early!

## Why

- Cloud Firestore is described as better than Readtime Database in every way, except that it is in beta and no Unity SDK yet.
- Decision to use Realtime Database or Firestore is a big forked path, since it affects the way you would design as hierarchy (Firestore) or flat with data duplications (RDB). There is probably 0% chance of easy migration. Unity devs will be faced with difficult decision of using RDB now and wait for SDK then having to overhaul design and migrate database, or just use Firestore with Firestorm while waiting for official SDK.
- The Unity SDK will likely be a packed DLL like it currently is at present. This had given me multiple headache where I could not fix the problem by myself without access to source code especially when upgrading to beta/alpha Unity build. Firestorm will be completely independent from Firebase Unity SDK, including the auth part before it can use Firestore.
- It does not cover anything other than Auth and Firestore. If you want to do Auth to Storage/Cloud Functions/etc. please just use the Unity SDK to auth again separately.
- Because it maintains only compatibility of Auth and Firestore maybe it is easier and faster to adopt a new Firestore feature added by Google.

## Requires

- gRPC Unity package.
- Unity 2019.1 (may work with 2018.3 but I have enough time to care about backward compatibility sorry..)
- C# 7.3