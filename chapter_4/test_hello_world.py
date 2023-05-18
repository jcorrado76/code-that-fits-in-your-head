# Third Party Imports
import pytest
from fastapi.testclient import TestClient
from hello_world import app
from loguru import logger


@pytest.fixture
def test_client():
    return TestClient(app)


def test_home_returns_json(test_client):
    response = test_client.get("/")
    assert response.status_code == 200
    assert response.json() == {"message": "Hello World"}
    assert response.headers["content-type"] == "application/json"


def test_can_post_data(test_client):
    test_json = {
        "date": "2023-03-10 19:00",
        "email": "katinka@example.com",
        "name": "Katinka Ingabogovinanana",
        "quantity": 2,
    }
    try:
        response = test_client.post("/reservation", json=test_json)
    except AssertionError:
        logger.error("Could not request /reservations resource")
    assert response.status_code == 200
    assert response.json() == test_json
