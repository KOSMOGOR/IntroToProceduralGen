using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PSOCellural : MonoBehaviour
{
    public SwarmParticleCellural swarpParticlePrefab;
    public Vector3 minStartVec = new(-5, -5, -5), maxStartVec = new(5, 5, 5);
    public CelluralAutomata automata;
    public Transform target;
    public float dt = 0.1f;

    public Vector3 globalBest;

    readonly List<SwarmParticleCellural> particles = new();
    bool spawned = false;

    void Update() {
        if (spawned || !automata.isFinished) return;
        spawned = true;
        CreateParticles();
        StartCoroutine(UpdateCoroutine());
    }

    IEnumerator UpdateCoroutine() {
        while (true) {
            globalBest = GetGlobalBest();
            particles.ForEach(p => p.UpdateParticle(globalBest, Evaluate, dt));
            yield return new WaitForSeconds(dt);
        }
    }

    void CreateParticles() {
        List<Rock> rocks = new();
        for (int x = 0; x < automata.x; ++x) {
            for (int y = 0; y < automata.y; ++y) {
                Rock rock = automata.rocks[x, y];
                if (rock.isRock) {
                    rocks.Add(rock);
                    continue;
                }
                SwarmParticleCellural particle = Instantiate(swarpParticlePrefab, transform);
                particle.transform.localPosition = rock.transform.position + Vector3.back;
                particle.velocity = RandomVector();
                particle.rockSize = automata.rockSize / 2;
                particles.Add(particle);
            }
        }
    }

    Vector3 RandomVector() {
        Vector3 vec = new();
        for (int i = 0; i < 3; ++i) vec[i] = Random.Range(minStartVec[i], maxStartVec[i]);
        vec.z = 0;
        return vec;
    }

    Vector3 GetGlobalBest() {
        List<Vector3> positions = particles.Select(p => p.transform.position).ToList();
        return positions.Select(p => new KeyValuePair<Vector3, float>(p, Evaluate(p))).OrderBy(p => p.Value).First().Key;
    }

    float Evaluate(Vector3 pos) {
        return Vector3.Distance(target.position, pos) - particles.Select(p => Vector3.Distance(p.transform.position, pos) + p.velocity.magnitude / 2).Sum() / particles.Count;
    }
}
