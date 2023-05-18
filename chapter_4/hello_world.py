# Third Party Imports
from fastapi import FastAPI
from pydantic import BaseModel

app = FastAPI()


@app.get("/")
async def get_hello_world():
    return {"message": "Hello World"}


class Reservation(BaseModel):
    date: str
    email: str
    name: str
    quantity: int


@app.post("/reservation")
async def post_reservation_to_db(reservation: Reservation):
    return reservation
