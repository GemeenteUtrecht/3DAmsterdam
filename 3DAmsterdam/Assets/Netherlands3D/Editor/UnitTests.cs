﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using ConvertCoordinates;
using System;

public class UnitTests 
{

    [Test]
    public void TestReplaceXY()
    {
        string template = "terrain/terrain_{x}_{y}.lod1.mesh";
        var result = template.ReplaceXY(444000, 123000);
        Assert.AreEqual("terrain/terrain_444000_123000.lod1.mesh", result);
    }

    [Test]
    public void TestGetRDCoordinate()
    {
        var testFilePaths1 = new string[]{
            "123000_443000_utrecht_lod1",
            "utrecht_123000_443000_lod1",
            "buildings_123000_443000.1.2",
            "terrain_123000-443000",
            "terrain_123000-443000-lod1",
            "trees_123000-443000",
            "trees_123000-443000-lod1",
        };

        foreach (var filePath in testFilePaths1)
        {
            var expectedRd = new Vector3RD(123000, 443000, 0);
            var result = filePath.GetRDCoordinate();
            Assert.AreEqual(expectedRd, result);
        }

        var testFilePaths2 = new string[]{
            "7000_392000_zee_land_lod1",
            "zee_land_7000_392000_lod1",
            "buildings_7000_392000.1.2",
            "terrain_7000-392000",
            "terrain_7000-392000-lod1",
            "trees_7000-392000",
            "trees_7000-392000-lod1",
        };

        foreach (var filePath in testFilePaths2)
        {
            var expectedRd = new Vector3RD(7000, 392000, 0);
            var result = filePath.GetRDCoordinate();
            Assert.AreEqual(expectedRd, result);
        }

        var testFilePaths3 = new string[]{
            "AssetBundles",
            "move.py"
        };

        foreach (var filePath in testFilePaths3)
        {
            Assert.Throws<Exception>(
             delegate { filePath.GetRDCoordinate(); });
        }

        Exception ex = Assert.Throws<Exception>(
            delegate { "AssetBundles".GetRDCoordinate(); });        

    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator NewTestScriptWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
        Assert.IsTrue(true);
    }



}
