using System;

namespace Chief.App.Models;

// TODO: title 未来替换成 key 用于本地化
public record PageLink(string Title, string Icon, Type Page);