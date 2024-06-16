from mitmproxy import http
from mitmproxy import ctx
import os

def request(flow: http.HTTPFlow) -> None:
    if flow.request.method == "POST" and "multipart/form-data" in flow.request.headers["Content-Type"]:
        # 获取 multipart 数据的边界
        boundary = flow.request.headers["Content-Type"].split("boundary=")[-1]

        # 获取 multipart 数据的原始内容
        raw_content = flow.request.get_content()

        # 解析 multipart 数据
        parts = raw_content.split(boundary.encode())

        for part in parts:
            if b'Content-Disposition: form-data;' in part:
                # 查找文件字段
                if b'filename=' in part:
                    headers, content = part.split(b'\r\n\r\n', 1)
                    filename = headers.split(b'filename="')[1].split(b'"')[0].decode()

                    # 保存文件内容
                    with open(filename, 'wb') as f:
                        f.write(content.rstrip(b'--\r\n'))
                    
                    ctx.log.info(f"Saved file: {filename}")
                else:
                    # 处理其他非文件字段
                    pass
