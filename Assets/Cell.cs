using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{

    public int height = 1;
    public bool water = false;
    bool is_selected = false;
    public float water_level = 0;
    Color unselected_color = new Color(0.9f, 0.9f, 0.5f);
    Color water_color = new Color(0f, 0f, 1f);
    Color selected_color = new Color(0f, 0f, 0f);
    
    public float light_level = 0;
    
    public GameObject entity;
    
    
    // Start is called before the first frame update
    void Start()
    {
        this.reset_color();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void set_height(int new_height){
        this.height = new_height;
        gameObject.transform.localScale = new Vector3(1,new_height,1);
        
        if (!(this.entity is null))
            this.entity.GetComponent<Entity>().set_height(new_height);
    }
    
    public void inc_height(){
        if (this.height < 5)
            this.set_height(this.height+1);
    }
    
        public void dec_height(){
        if (this.height > 1)
            this.set_height(this.height-1);
    }
    
    public void selected(){
        this.is_selected = true;
        this.reset_color();
    }
    
    public void unselected(){
        this.is_selected = false;
        this.reset_color();
    }
    
    public void create_water(){
        this.water = true;
    }
    
    public void destroy_water(){
        this.water = false;
    }
    
    public void reset_color(){
    
        Color color;
        
        color = Color.Lerp(this.unselected_color, this.water_color, this.water_level);
        
        
        if (this.is_selected){
            color = Color.Lerp(color, this.selected_color, 0.5f);
        } else {
            color = color;
        }
        
        gameObject.GetComponent<Renderer>().material.color = color;
    }
    
    public void seed(int grown_tick){
        this.seed(Entity.Type.SEED, grown_tick);
    }

    public void seed(Entity.Type type, int grown_tick){
        this.remove_entity();

        this.entity = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
        Vector3 position = gameObject.transform.position;
        this.entity.AddComponent<Entity>();
        this.entity.GetComponent<Entity>().set_position(position.x, position.z, this.height);
        this.entity.GetComponent<Entity>().set_type(type);
        this.entity.GetComponent<Entity>().grown_tick = grown_tick;

    }
    
    public void remove_entity(){
        Destroy(this.entity);
        this.entity = null;
    }
    
    public void act(Game game, int x, int y){
        if (this.entity != null)
            this.entity.GetComponent<Entity>().act(game, x, y);
    }
    
}
