#pragma once

#include <iostream>
#include <vector>
#include <memory>
#include <algorithm>
#include <bitset>
#include <array>

#include "GameObject.h"

//Components like Transform
class Component;
class Entity;

//We are saying that ComponentID is able to store the maxumum size of a theoretical possible object of any type (including an array.)
using ComponentID = std::size_t;


inline ComponentID GetComponentTypeID()
{
    static ComponentID lastID = 0;
    return lastID++;
};


template <typename T>
inline ComponentID GetComponentTypeID() noexcept
{
    static ComponentID typeID = GetComponentTypeID();
    return typeID;
}

//The max amount of components an entity can hold;
constexpr std::size_t maxComponents = 32;


//Allows us to find out if an entity has got a selection of components
using ComponentBitSet = std::bitset<maxComponents>;
//Array of component pointers
using ComponentArray = std::array<Component*, maxComponents>;

class Component
{
public:
    //Reference to it's owner
    Entity* entity;

    virtual void Init(){}
    virtual void Update(){}
    virtual void Draw(){}

    virtual ~Component(){}
};


class Entity
{
protected:

    
private:
    //If not active then delete
    bool active = true;
    //Vectors are equivalent to C# List<T>
    std::vector<std::unique_ptr<Component>> components;
    ComponentArray componentArray;
    ComponentBitSet bitSet;

public:

    void ComponentUpdate()
    {
        for (auto& c : components) c->Update();
    }


    
   virtual  void Update(){}
    

    virtual void Draw()
    {
        for (auto& c : components) c->Draw();
    }

    virtual bool IsActive()
    {
        return active;
    }

    virtual void Destroy()
    {
        active = false;
    }


    template <typename T>
    bool HasComponent() const
    {
        return bitSet[GetComponentTypeID<T>];
    }
    
    template <typename T, typename... TArgs> T& AddComponent(TArgs&&... mArgs)
    {
        T* c(new T(std::forward<TArgs>(mArgs)...));
        c->entity = this;
        std::unique_ptr<Component> uPtr{c};
        components.emplace_back(std::move(uPtr));
        
        componentArray[GetComponentTypeID<T>()] = c;
        bitSet[GetComponentTypeID<T>()] = true;

        c->Init();

        return *c;
    }

    template<typename T> T& GetComponent() const
    {
        auto ptr(componentArray[GetComponentTypeID<T>()]);
        return *static_cast<T*>(ptr);
    }
};

class Manager
{
private:
    std::vector<std::unique_ptr<Entity>> entities_;
  

public:
    void Update()
    {
        for(auto &e : entities_)
        {
            e->ComponentUpdate();
            e->Update();
        }
        
    }

    void Draw()
    {
        for(auto &e : entities_)e->Draw();
    }

    void Refresh()
    {
        //Will loop through the entities
        entities_.erase(std::remove_if(std::begin(entities_), std::end(entities_),
            [](const std::unique_ptr<Entity> &mEntity)
            {
                return !mEntity->IsActive();
            }),
            std::end(entities_));
    }


    Entity& AddEntity()
    {
        //Create a new Entity
        Entity* e = new Entity();
        //Create a Unique Ptr of the entity. std::unique_ptr is a smart pointer that owns and manages another object through a pointer and disposes of that object when the unique_ptr goes out of scope. 
        std::unique_ptr<Entity> uPtr{e};
        //Add it into the vector
        entities_.emplace_back(std::move(uPtr));

        //return the reference
        return *e;
        
    }
};



