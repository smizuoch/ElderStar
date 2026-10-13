// Measurement fixture only; never included in the NOVA package or avatar.
#include <d3d11.h>
#include <cstdint>
#include <deque>
#include <mutex>
#include "IUnityInterface.h"
#include "IUnityGraphics.h"
#include "IUnityGraphicsD3D11.h"

static IUnityInterfaces* interfaces;
static ID3D11DeviceContext* context;
struct Query { ID3D11Query *start = nullptr, *stop = nullptr, *disjoint = nullptr; bool pending = false; int block = -1; };
static Query queries[64];
static int current = -1;
struct Result { int block; double milliseconds; };
static std::deque<Result> results;
static std::mutex resultMutex;

static void Collect()
{
    for (auto& q : queries)
    {
        if (!q.pending) continue;
        D3D11_QUERY_DATA_TIMESTAMP_DISJOINT frequency{};
        UINT64 start = 0, stop = 0;
        if (context->GetData(q.disjoint, &frequency, sizeof(frequency), D3D11_ASYNC_GETDATA_DONOTFLUSH) != S_OK ||
            context->GetData(q.start, &start, sizeof(start), D3D11_ASYNC_GETDATA_DONOTFLUSH) != S_OK ||
            context->GetData(q.stop, &stop, sizeof(stop), D3D11_ASYNC_GETDATA_DONOTFLUSH) != S_OK) continue;
        q.pending = false;
        if (frequency.Disjoint || frequency.Frequency == 0 || stop <= start) continue;
        std::lock_guard<std::mutex> lock(resultMutex);
        if (results.size() < 4096) results.push_back({q.block, 1000.0 * (stop - start) / frequency.Frequency});
    }
}

static void UNITY_INTERFACE_API RenderEvent(int event, void* data)
{
    if (!context)
    {
        if (interfaces->Get<IUnityGraphics>()->GetRenderer() != kUnityGfxRendererD3D11) return;
        auto device = interfaces->Get<IUnityGraphicsD3D11>()->GetDevice();
        device->GetImmediateContext(&context);
        for (auto& q : queries)
        {
            D3D11_QUERY_DESC stamp{D3D11_QUERY_TIMESTAMP, 0}, disjoint{D3D11_QUERY_TIMESTAMP_DISJOINT, 0};
            if (FAILED(device->CreateQuery(&stamp, &q.start)) || FAILED(device->CreateQuery(&stamp, &q.stop)) ||
                FAILED(device->CreateQuery(&disjoint, &q.disjoint))) return;
        }
    }
    if (event == 0)
    {
        Collect(); current = -1;
        for (int i = 0; i < 64; ++i) if (!queries[i].pending && queries[i].disjoint && queries[i].start && queries[i].stop) { current = i; break; }
        if (current < 0) return;
        auto& q = queries[current]; q.block = static_cast<int>(reinterpret_cast<intptr_t>(data));
        context->Begin(q.disjoint); context->End(q.start);
    }
    else if (event == 1 && current >= 0)
    {
        auto& q = queries[current]; context->End(q.stop); context->End(q.disjoint); q.pending = true;
        current = -1;
        // Hidden Players have no Present; submit the offscreen work explicitly.
        context->Flush(); Collect();
    }
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unity) { interfaces = unity; }
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    for (auto& q : queries) { if (q.start) q.start->Release(); if (q.stop) q.stop->Release(); if (q.disjoint) q.disjoint->Release(); q = {}; }
    if (context) context->Release(); context = nullptr;
}
extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API NovaGetTimerEvent() { return RenderEvent; }
extern "C" int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API NovaPopGpuTime(int* block, double* milliseconds)
{
    std::lock_guard<std::mutex> lock(resultMutex);
    if (results.empty()) return 0;
    *block = results.front().block; *milliseconds = results.front().milliseconds; results.pop_front(); return 1;
}
