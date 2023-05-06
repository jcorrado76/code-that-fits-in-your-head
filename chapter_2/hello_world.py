from fastapi import FastAPI

app = FastAPI()


@app.get("/")
async def get_hello_world():
    return {"message": "Hello World"}
