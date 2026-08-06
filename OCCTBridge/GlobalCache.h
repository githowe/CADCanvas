#pragma once

/// <summary>
/// 全局缓存
/// </summary>
class GlobalCache
{
private:
	GlobalCache() = default;
	~GlobalCache() = default;

public:
	static GlobalCache& GetInstance()
	{
		static GlobalCache instance;
		return instance;
	}
	// 禁止拷贝构造和赋值操作
	GlobalCache(const GlobalCache&) = delete;
	GlobalCache& operator=(const GlobalCache&) = delete;

public:
	/// <summary>坐标缓存</summary>
	double* Point2DCache = nullptr;
	int Point2DCacheSize = 0;
};