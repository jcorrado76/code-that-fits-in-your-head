# Third Party Imports
import pytest
from fastapi.testclient import TestClient
from hello_world import app


@pytest.fixture
def test_client():
    return TestClient(app)


def test_get_hello_world(test_client):
    response = test_client.get("/")
    assert response.status_code == 200
    assert response.json() == {"message": "Hello World"}
